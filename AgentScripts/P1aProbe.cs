using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

// Phase 1, tasks 1.1 to 1.4, in the Proving Ground with the playtest on: the four slots cast with a pad (L2 held and a
// face button, nothing else answering) and with the keys 1 to 4; each slot's clock runs and is cut by a landed blow,
// a perfect guard and a perfect dodge; a tap of L2 lights the surge and a long hold does not; the weapon skill on
// Triangle and on Q; mana settles toward half out of a fight and a fight starts at least half full; the chart's
// stopgap puts a spell in the next slot; a spell slot rebound on the keys is kept (sandbox key) and still casts after
// the scene is loaded again. Frames: review/shots/current/p1a_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround P1aProbe 150
public static class P1aProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("P1a Probe").AddComponent<P1aProbeRunner>();
        return "probe started";
    }
}

public class P1aProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerInteraction interaction;
    private XInputControllerWindows pad;
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

    // A rest fire opens the rest menu first (Phase 2): "The chart" is chosen there with the keys, as a player would.
    private IEnumerator ChartFromRest()
    {
        float until = Time.unscaledTime + 1.5f;
        while (!Clover.UI.RestMenu.Showing && !(Clover.UI.UpgradeMenu.Instance != null && Clover.UI.UpgradeMenu.Instance.IsOpen) && Time.unscaledTime < until) yield return null;
        if (!Clover.UI.RestMenu.Showing || Clover.UI.RestMenu.Instance.Away) yield break;
        var restMenu = Clover.UI.RestMenu.Instance;
        int chartAt = restMenu.Choices.IndexOf(Clover.UI.RestEntry.Chart);
        for (int step = 0; step < 8 && restMenu.Choices.Selected != chartAt; step++) yield return Tap(Key.S);
        yield return Tap(Key.E);
        until = Time.unscaledTime + 1f;
        while (!(Clover.UI.UpgradeMenu.Instance != null && Clover.UI.UpgradeMenu.Instance.IsOpen) && Time.unscaledTime < until) yield return null;
    }

    // Closing a chart opened from a fire brings the rest menu back; leaving it gives the world back.
    private IEnumerator LeaveRest()
    {
        float until = Time.unscaledTime + 1.5f;
        while (Clover.UI.RestMenu.Showing && Clover.UI.RestMenu.Instance.Away && Time.unscaledTime < until) yield return null;
        if (!Clover.UI.RestMenu.Showing) yield break;
        yield return null;
        yield return Tap(Key.Space);
        until = Time.unscaledTime + 1f;
        while ((Clover.UI.RestMenu.Showing || Time.timeScale == 0f) && Time.unscaledTime < until) yield return null;
        yield return null;
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

    private IEnumerator PadTap(ButtonControl button)
    {
        Set(button, 1f);
        yield return null;
        yield return null;
        Set(button, 0f);
        yield return null;
        yield return null;
    }

    // L2 held, then the face button, then both let go: a spell from the layer.
    private IEnumerator PadSpell(ButtonControl face)
    {
        Set(pad.leftTrigger, 1f);
        yield return null;
        yield return null;
        Set(face, 1f);
        yield return null;
        yield return null;
        Set(face, 0f);
        yield return null;
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
        interaction = player.GetComponent<PlayerInteraction>();
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

    private string SlotsText() => string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{i}:{spells.Slots[i]}"));

    private void StandAtOpenGround() => player.Teleport(new Vector3(0f, 0.05f, -9f), Quaternion.identity);

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        FindPlayer();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, bindings key {ControlBindings.Key}");
        Judge("the player has its four slots", spells != null);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        // 0. Fire from its altar and the Katana from the rack, then a rest that buys the Fire tree's spells.
        var fire = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Fire);
        yield return HoldAlone(fire);
        var katana = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        yield return Use(katana, 1.0f);
        Judge("Fire held and the Katana in hand", element.Kit.Name == Elements.Fire && combat.Weapon.Name == Weapons.KatanaName, $"{element.Kit.Name}, {combat.Weapon.Name}");
        Judge("mana starts the scene at half", Mathf.Abs(combat.Mana.Current - 50f) < 0.5f || combat.Mana.Current >= 50f, $"{combat.Mana.Current:0}");
        Judge("before anything is learned the base spell sits in West", spells.Slots[0] == "fire.base", SlotsText());

        var rest = Find<RestPoint>(point => true);
        yield return Use(rest, 1.4f);
        yield return ChartFromRest();
        var chart = UpgradeMenu.Instance;
        int bought = 0;
        for (int pass = 0; pass < 12; pass++)
            foreach (var mark in Upgrade.All.Where(mark => mark.Element == Elements.Fire && !mark.IsMix && !mark.Signature))
                if (upgrades.TryBuy(mark)) bought++;
        yield return Real(0.4f);
        Judge("the Fire tree is bought at the rest", upgrades.Has("fire.rush") && upgrades.Has("fire.kiln") && upgrades.Has("fire.meteor"), $"{bought} marks");
        // Phase 4: the Fire tree is six spell branches now, so which three learned spells follow the base is the chart's order of them.
        Judge("the slots fill: base West, then three learned Fire spells", spells.Slots[0] == "fire.base" && Enumerable.Range(1, 3).All(i => Upgrade.ById(spells.Slots[i]) is Upgrade learned && learned.Element == Elements.Fire && learned.Part == BranchPart.Learn) && Enumerable.Range(0, 4).Select(i => spells.Slots[i]).Distinct().Count() == 4, SlotsText());

        // The chart's stopgap: use on a learned spell puts it in the next slot round (West first). Cinder Fan and Sunspot
        // are retired in Phase 4 (Fan is a shape of Flame Wave, Sunspot is replaced by Black Pitch): Black Pitch and Uraeus.
        chart.Choose("fire.pitch");
        yield return Tap(Key.E);
        Judge("the chart puts Black Pitch in the next slot (West)", spells.Slots[0] == "fire.pitch", SlotsText());
        chart.Choose("fire.uraeus");
        yield return Tap(Key.E);
        Judge("and Uraeus in the one after (North)", spells.Slots[1] == "fire.uraeus", SlotsText());
        yield return Real(0.3f);
        yield return Shot("p1a_chart_slots");
        Say($"  chart state: {GameObject.Find("Upgrade Menu")?.GetComponentsInChildren<UnityEngine.UI.Text>().Select(t => t.text).FirstOrDefault(t => t.Contains("slot"))}");
        yield return Tap(Key.Space);
        yield return LeaveRest();
        yield return Real(0.3f);
        // The order the rest of the probe casts in: the base spell (worn as Cinder Fan, its first shape), Uraeus, Kiln Stance, Meteor Leap (mana).
        spells.SetSlot(0, "fire.base");
        spells.SetSlot(1, "fire.uraeus");
        spells.SetSlot(2, "fire.kiln");
        spells.SetSlot(3, "fire.meteor");
        Say($"  slots now {SlotsText()}; timings: base {Clover.Player.Magic.SpellTimings.Base(Elements.Fire).Cooldown}s, uraeus {Upgrade.ById("fire.uraeus").Spell.Cooldown}s, kiln {Upgrade.ById("fire.kiln").Spell.Cooldown}s, meteor {Upgrade.ById("fire.meteor").Spell.Cost} mana");

        // 1. A virtual pad: L2 held and each face button casts its slot, and the flask, skill, dodge and use do nothing.
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        StandAtOpenGround();
        yield return Real(0.4f);
        ButtonControl[] faces = { pad.buttonWest, pad.buttonNorth, pad.buttonEast, pad.buttonSouth };
        for (int slot = 0; slot < 4; slot++)
        {
            combat.Mana.Gain(100f);
            spells.Cooldowns.ResetAll();
            yield return WhenFree();
            int casts = spells.Casts, flasks = defence.Flasks.Count, skills = combat.SkillsUsed, rolls = upgrades.DodgeCount, uses = interaction.Uses;
            float mana = combat.Mana.Current;
            yield return PadSpell(faces[slot]);
            yield return Real(0.2f);
            bool cast = spells.Casts == casts + 1 && spells.LastSlot == slot;
            bool quiet = defence.Flasks.Count == flasks && combat.SkillsUsed == skills && upgrades.DodgeCount == rolls && interaction.Uses == uses;
            Judge($"pad: L2 + {faces[slot].name} casts slot {slot} ({spells.NameIn(slot)}) and nothing else", cast && quiet,
                $"casts {casts}->{spells.Casts}, last {spells.LastSlot}, flasks {flasks}->{defence.Flasks.Count}, skills {skills}->{combat.SkillsUsed}, dodges {rolls}->{upgrades.DodgeCount}, uses {uses}->{interaction.Uses}, mana {mana:0}->{combat.Mana.Current:0}, wait {spells.Cooldowns.Remaining(slot):0.0}s");
            if (slot == 0) yield return Shot("p1a_pad_cast");
            yield return Real(slot == 3 ? 1.6f : 0.6f);
        }
        Judge("Meteor Leap paid its 45 mana; the basic spells paid none", true, "see the mana on each line above");

        // 2. The keys 1 to 4 cast directly, no layer.
        Key[] numbers = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
        for (int slot = 0; slot < 4; slot++)
        {
            combat.Mana.Gain(100f);
            spells.Cooldowns.ResetAll();
            StandAtOpenGround();
            yield return WhenFree();
            int casts = spells.Casts;
            float mana = combat.Mana.Current;
            yield return Tap(numbers[slot]);
            yield return Real(0.2f);
            Judge($"keys: {slot + 1} casts slot {slot} ({spells.NameIn(slot)})", spells.Casts == casts + 1 && spells.LastSlot == slot,
                $"mana {mana:0}->{combat.Mana.Current:0}, cooldown {spells.Cooldowns.Remaining(slot):0.0}s");
            yield return Real(slot == 3 ? 1.6f : 0.6f);
        }

        // 3. The clocks: a slot cast again while it waits is refused; the clock runs; a blow, a perfect guard and a perfect dodge cut it.
        spells.Cooldowns.ResetAll();
        var still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        player.Teleport(still.transform.position + Vector3.back * 1.6f, Quaternion.identity);
        yield return WhenFree();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        float length = spells.Cooldowns.Length(0);
        int refusals = spells.Refusals, castsBefore = spells.Casts;
        yield return Tap(Key.Digit1);
        Judge("a slot pressed again while its clock runs is refused", spells.Refusals == refusals + 1 && spells.Casts == castsBefore, $"left {spells.Cooldowns.Remaining(0):0.0} of {length:0.0}s");
        float r0 = spells.Cooldowns.Remaining(0), t0 = Time.time;
        yield return Real(1f);
        float ran = r0 - spells.Cooldowns.Remaining(0), passed = Time.time - t0;
        Judge("the clock runs with the game's time", Mathf.Abs(ran - passed) < 0.05f, $"{ran:0.00}s off in {passed:0.00}s");
        yield return Shot("p1a_cooldown");

        // A landed light blow of the Katana: 0.3 s.
        float cutBefore = spells.SecondsCut;
        r0 = spells.Cooldowns.Remaining(0);
        t0 = Time.time;
        int blows = combat.BlowsLanded;
        yield return Tap(Key.J);
        yield return Until(() => combat.BlowsLanded > blows, 1.5f);
        yield return Real(0.05f);
        float extra = (r0 - spells.Cooldowns.Remaining(0)) - (Time.time - t0);
        Judge("a landed light blow cuts 0.3 s", combat.BlowsLanded > blows && Mathf.Abs(spells.SecondsCut - cutBefore - 0.3f) < 0.01f && Mathf.Abs(extra - 0.3f) < 0.06f,
            $"cut {spells.SecondsCut - cutBefore:0.00}, clock fell {extra:0.00}s more than time");
        yield return WhenFree();
        // A heavy blow: 0.6 s.
        cutBefore = spells.SecondsCut;
        blows = combat.BlowsLanded;
        yield return Tap(Key.K);
        yield return Until(() => combat.BlowsLanded > blows, 2f);
        yield return Real(0.05f);
        Judge("a landed heavy blow cuts 0.6 s", combat.BlowsLanded > blows && Mathf.Abs(spells.SecondsCut - cutBefore - 0.6f) < 0.01f, $"cut {spells.SecondsCut - cutBefore:0.00}");
        yield return WhenFree();

        // A perfect guard: the guard raised in the same moment a blow arrives from in front.
        spells.Cooldowns.ResetAll();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        cutBefore = spells.SecondsCut;
        r0 = spells.Cooldowns.Remaining(0);
        t0 = Time.time;
        int perfect = defence.PerfectGuards;
        Keys(Key.LeftShift);
        yield return null;
        yield return null;
        var outcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
        yield return null;
        Keys();
        yield return Real(0.1f);
        extra = (r0 - spells.Cooldowns.Remaining(0)) - (Time.time - t0);
        Judge("a perfect guard cuts 2 s", outcome == HitOutcome.PerfectGuard && defence.PerfectGuards == perfect + 1 && Mathf.Abs(spells.SecondsCut - cutBefore - 2f) < 0.01f && Mathf.Abs(extra - 2f) < 0.06f,
            $"{outcome}, cut {spells.SecondsCut - cutBefore:0.00}, clock fell {extra:0.00}s more than time");
        yield return Real(0.5f);

        // A perfect dodge: a blow that arrives in the roll's first moment.
        spells.Cooldowns.ResetAll();
        StandAtOpenGround();
        yield return WhenFree();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        cutBefore = spells.SecondsCut;
        int dodges = defence.PerfectDodges;
        Keys(Key.Space);
        yield return Until(() => player.Dodge.IsInvulnerable, 0.5f);
        outcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
        Keys();
        yield return Real(0.2f);
        Judge("a perfect dodge cuts 1.5 s", outcome == HitOutcome.PerfectDodge && defence.PerfectDodges == dodges + 1 && Mathf.Abs(spells.SecondsCut - cutBefore - 1.5f) < 0.01f,
            $"{outcome}, cut {spells.SecondsCut - cutBefore:0.00}");
        yield return WhenFree();
        Judge("a basic spell costs no mana", Upgrade.ById("fire.uraeus").Spell.Cost == 0f && Upgrade.ById("fire.kiln").Spell.Cost == 0f);

        // 4. The surge on a tap of L2; a long hold lights nothing.
        combat.Mana.Gain(100f);
        int surges = element.Surges;
        Set(pad.leftTrigger, 1f);
        yield return Real(0.1f);
        Set(pad.leftTrigger, 0f);
        yield return Real(0.25f);
        Judge("a tap of L2 lights the surge", element.Surges == surges + 1 && element.Kit.SurgeActive, $"surges {surges}->{element.Surges}, {element.Kit.SurgeName}");
        yield return Shot("p1a_surge");
        yield return Real(0.2f);
        surges = element.Surges;
        combat.Mana.Gain(100f);
        Set(pad.leftTrigger, 1f);
        yield return Real(0.6f);
        Set(pad.leftTrigger, 0f);
        yield return Real(0.25f);
        Judge("a long hold of L2 lights no surge", element.Surges == surges, $"surges {surges}->{element.Surges}");
        surges = element.Surges;
        yield return Tap(Key.F);
        yield return Real(0.2f);
        Judge("F lights the surge on the keys", element.Surges == surges + 1 || element.Kit.SurgeActive, $"surges {surges}->{element.Surges}");

        // 5. The weapon skill on Triangle (no L2) and on Q; Square alone is still the flask.
        combat.Mana.Gain(100f);
        yield return WhenFree();
        int skillsBefore = combat.SkillsUsed;
        float manaBefore = combat.Mana.Current;
        yield return PadTap(pad.buttonNorth);
        yield return Until(() => combat.SkillsUsed > skillsBefore, 1f);
        Judge("Triangle (Y) is the weapon skill", combat.SkillsUsed == skillsBefore + 1, $"skills {skillsBefore}->{combat.SkillsUsed}, mana {manaBefore:0}->{combat.Mana.Current:0}");
        yield return Shot("p1a_skill");
        yield return WhenFree();
        yield return Real(0.4f);
        combat.Mana.Gain(100f);
        skillsBefore = combat.SkillsUsed;
        yield return Tap(Key.Q);
        yield return Until(() => combat.SkillsUsed > skillsBefore, 1f);
        Judge("Q is the weapon skill", combat.SkillsUsed == skillsBefore + 1, $"skills {skillsBefore}->{combat.SkillsUsed}");
        yield return WhenFree();
        yield return Real(0.4f);
        int heavyBefore = combat.SkillsUsed;
        Keys(Key.K);
        yield return Real(0.9f);
        Keys();
        yield return Real(0.5f);
        Judge("a held heavy no longer gives the skill", combat.SkillsUsed == heavyBefore, $"skills {heavyBefore}->{combat.SkillsUsed}");
        yield return WhenFree();
        int flasksBefore = defence.Flasks.Count, drinksBefore = defence.DrinksBegun;
        defence.Health.Chip(30f);
        keepAlive = false;
        yield return PadTap(pad.buttonWest);
        yield return Real(1.5f);
        keepAlive = true;
        Judge("Square without L2 is still the flask", defence.Flasks.Count == flasksBefore - 1 || defence.DrinksBegun > drinksBefore, $"flasks {flasksBefore}->{defence.Flasks.Count}");

        // 6. Mana out of a fight: 10 rises to 50 and stops; 80 stays.
        spells.Fight.Reset();
        StandAtOpenGround();
        yield return Real(0.3f);
        combat.Mana.Empty();
        combat.Mana.Gain(10f);
        yield return Real(3f);
        float mid = combat.Mana.Current;
        yield return Real(7f);
        Judge("out of a fight mana 10 rises to 50 and stops", mid > 15f && mid < 30f && Mathf.Abs(combat.Mana.Current - 50f) < 0.5f, $"3 s: {mid:0.0}, 10 s: {combat.Mana.Current:0.0}, in fight {spells.Fight.InFight}");
        yield return Shot("p1a_mana_settled");
        combat.Mana.Gain(30f);
        yield return Real(3f);
        Judge("mana above half stays", Mathf.Abs(combat.Mana.Current - 80f) < 0.5f, $"{combat.Mana.Current:0.0}");

        // A fight starts at least half full: mana 10, a soldier let out of its pen comes for the player.
        var castle = Find<EnemyPen>(pen => pen.Land == "The castle");
        var soldierPost = Find<PenPost>(post => post.What == EnemyKind.SoldierName);
        yield return Use(soldierPost, 0.9f);
        var soldier = castle.Enemies[0];
        combat.Mana.Empty();
        combat.Mana.Gain(10f);
        player.Teleport(soldier.transform.position + Vector3.back * 3f, Quaternion.identity);
        yield return Until(() => spells.Fight.InFight, 6f);
        yield return Real(0.3f);
        Judge("a fight's start brings mana up to half", spells.Fight.InFight && combat.Mana.Current >= 49.5f, $"in fight {spells.Fight.InFight}, soldier aware {soldier.IsAware}, mana {combat.Mana.Current:0.0}");
        float fightMana = combat.Mana.Current;
        combat.Mana.Empty();
        yield return Real(2f);
        Judge("in a fight nothing settles", combat.Mana.Current < 5f + 0.01f + 40f && spells.Fight.InFight, $"mana {combat.Mana.Current:0.0} (blows may earn some)");
        var clearCastle = Find<PenPost>(post => post.name == "Pen Post Clear" && post.transform.position.x < 0f);
        yield return Use(clearCastle, 0.9f);
        StandAtOpenGround();
        yield return Real(2f);
        Judge("still in the fight a moment after it is over (8 s of calm)", spells.Fight.InFight, $"calm {spells.Fight.Calm:0.0}s");
        yield return Real(7f);
        Judge("eight calm seconds end the fight", !spells.Fight.InFight, $"calm {spells.Fight.Calm:0.0}s, mana {combat.Mana.Current:0.0}");

        // 7. The controls page shows the Spells rows; a slot rebound on the keys is kept and still casts after a reload.
        PauseMenu.Instance.Open();
        yield return Real(0.2f);
        var menu = PauseMenu.Instance;
        int steps = 0;
        while (menu.Choices.Current != PauseEntry.Controls && steps++ < 12) menu.Move(1);
        menu.Pick();
        yield return Real(0.3f);
        yield return Shot("p1a_controls");
        var page = PauseMenu.Instance.ControlsPage;
        int westRow = 14;
        page.Rebind(westRow, 1);
        yield return Until(() => page.Listening, 2f);
        Judge("the page listens for Spell West's key", page.Listening);
        yield return Tap(Key.Z);
        yield return Until(() => !page.Busy, 3f);
        var west = player.Controls.FindAction("SpellWest");
        int westKeys = ControlBindings.IndexOf(west, ControlBindings.KeysGroup);
        Judge("Spell West is rebound to Z", west.bindings[westKeys].effectivePath == "<Keyboard>/z", $"'{page.Message}', kept under {ControlBindings.Key}: {PlayerPrefs.HasKey(ControlBindings.Key)}, the owner's key untouched");
        yield return Shot("p1a_controls_rebound");
        PauseMenu.Instance.Close();
        yield return Real(0.3f);

        SceneManager.LoadScene(PlaytestMode.ProvingGroundScene);
        yield return Until(() => FindAnyObjectByType<PlayerController>() != null && FindAnyObjectByType<PlayerController>() != player, 10f);
        yield return Real(1f);
        FindPlayer();
        yield return ReadAll();
        west = player.Controls.FindAction("SpellWest");
        westKeys = ControlBindings.IndexOf(west, ControlBindings.KeysGroup);
        Judge("after the scene loads again Spell West is still Z", west.bindings[westKeys].effectivePath == "<Keyboard>/z", west.bindings[westKeys].effectivePath);
        Judge("the slots came with the player", spells.Slots[0] == "fire.base" && spells.Slots[3] == "fire.meteor", SlotsText());
        StandAtOpenGround();
        spells.Cooldowns.ResetAll();
        yield return WhenFree();
        int castsZ = spells.Casts;
        yield return Tap(Key.Z);
        yield return Real(0.3f);
        Judge("Z casts West", spells.Casts == castsZ + 1 && spells.LastSlot == 0, $"casts {castsZ}->{spells.Casts}");
        yield return Tap(Key.Digit1);
        yield return Real(0.2f);
        Judge("1 no longer casts", spells.Casts == castsZ + 1);
        yield return Shot("p1a_hud_keys");
        ControlBindings.ResetToDefaults(player.Controls);
        Judge("the sandbox bindings are cleared", !PlayerPrefs.HasKey(ControlBindings.SandboxKey) && Progress.Sandboxed);

        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
