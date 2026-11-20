using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees.Duo;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The enemy's-blow moment (the ledger's their-blow-moment, the owner's decision of 2026-11-20), in the Proving Ground
// with the playtest on: The Grudge's cloud strikes when the enemy it hangs over next strikes at you, whether the blow
// lands, is guarded or is turned by a ward, and not before; Ash Plume's storm falls on that blow too. Each judged line
// ends OK or WRONG. Progress stays sandboxed.
//   bash AgentScripts/runprobe.sh ProvingGround GrudgeProbe 60
public static class GrudgeProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Grudge Probe").AddComponent<GrudgeProbeRunner>();
        return "probe started";
    }
}

public class GrudgeProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy still, host;
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);
    private static readonly int Grudge = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Static);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

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

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while ((ReadingBox.Instance != null && ReadingBox.Instance.IsOpen || GuidePage.Showing) && guard++ < 60)
        {
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return null;
            yield return null;
        }
    }

    // A fresh host with the cloud over it: Cursed, then two Static, and The Grudge goes off. The player stands 1.5 m south.
    private IEnumerator Clouded()
    {
        if (host != null) Destroy(host.gameObject);
        yield return null;
        host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Probe Host";
        yield return Until(() => Hazards.Alive == 0, 3f);
        player.Teleport(Middle + Vector3.back * 1.5f, Quaternion.identity);
        defence.Restore();
        yield return Real(0.3f);
        int shown = reactions.TimesShown(Grudge);
        host.Afflictions.Lay(AfflictionKind.Cursed, 1, 0f, 1f, LaySource.Spell, host);
        yield return null;
        host.Afflictions.Lay(AfflictionKind.Static, 3, 0f, 1f, LaySource.Spell, host);
        yield return Until(() => reactions.TimesShown(Grudge) > shown, 1.5f);
        yield return Real(0.2f);
    }

    private bool Cloud(string id) => host != null && host.Afflictions.Conditions.Holds(id);

    // The host's blow at the player, from where it stands.
    private HitOutcome TheirBlow()
    {
        Vector3 from = host.transform.position;
        Vector3 toward = (player.transform.position - from);
        toward.y = 0f;
        return defence.TakeHit(new Hit(4f, 0f, toward.normalized, false, false, from));
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
        upgrades = player.GetComponent<PlayerUpgrades>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        build.Attune(Elements.Dark, Elements.Lightning);
        upgrades.RefreshNow();

        // The plain Grudge: the cloud waits, and the enemy's blow that lands calls it down.
        yield return Clouded();
        Judge("The Grudge hangs its cloud over the enemy", Cloud(Clover.Player.Trees.Reactions.Cloud) && reactions.LastShape == "", $"shape '{reactions.LastShape}'");
        int staticBefore = host.Afflictions.Stacks(AfflictionKind.Static);
        float healthBefore = host.Health.Current;
        yield return Real(1f);
        Judge("the cloud waits while the enemy does not strike", Cloud(Clover.Player.Trees.Reactions.Cloud) && host.Afflictions.Stacks(AfflictionKind.Static) == staticBefore);
        int blows = upgrades.TheirBlows;
        var outcome = TheirBlow();
        yield return Real(0.3f);
        Judge("the enemy's blow is heard as their blow", upgrades.TheirBlows == blows + 1, $"outcome {outcome}");
        Judge("its blow lands on you and the cloud strikes it: its Static discharges", host.Afflictions.Stacks(AfflictionKind.Static) == 0 && host.Health.Current < healthBefore - 20f,
            $"Static {staticBefore} -> {host.Afflictions.Stacks(AfflictionKind.Static)}, health lost {healthBefore - host.Health.Current:0}");
        Judge("it is stopped for a moment", host.Afflictions.IsHeld);
        Judge("and the cloud is spent", !Cloud(Clover.Player.Trees.Reactions.Cloud));

        // A blow on the guard calls it down as well.
        yield return Clouded();
        player.Stamina.Fill();
        Keys(Key.L);
        yield return Real(0.5f);
        staticBefore = host.Afflictions.Stacks(AfflictionKind.Static);
        outcome = TheirBlow();
        yield return null;
        Keys();
        yield return Real(0.3f);
        Judge("a blow held on the guard calls the cloud down too", outcome == HitOutcome.Blocked && host.Afflictions.Stacks(AfflictionKind.Static) == 0 && !Cloud(Clover.Player.Trees.Reactions.Cloud),
            $"outcome {outcome}, Static {staticBefore} -> {host.Afflictions.Stacks(AfflictionKind.Static)}");

        // A ward that turns the blow, the same.
        yield return Clouded();
        defence.GiveWards(1);
        outcome = TheirBlow();
        yield return Real(0.3f);
        Judge("a blow turned by a ward calls it down too", host.Afflictions.Stacks(AfflictionKind.Static) == 0 && !Cloud(Clover.Player.Trees.Reactions.Cloud), $"outcome {outcome}");

        // Ash Plume: the storm falls on the enemy's blow, one bolt for each of your blows on the cloud.
        upgrades.Give(Upgrade.ById(TheGrudgeDuo.Shape));
        upgrades.RefreshNow();
        Judge("Ash Plume chosen at a rest fire", build.ChooseShape(ReactionTable.ShapeKey(Grudge), 1, true));
        yield return Clouded();
        Judge("The Grudge runs in Ash Plume", reactions.LastShape == "Ash Plume" && Cloud(TheGrudgeDuo.AshPlume), $"shape '{reactions.LastShape}'");
        for (int i = 0; i < 3; i++)
        {
            host.TakeHit(new Hit(1f, 0f, Vector3.forward));
            yield return Real(0.1f);
        }
        healthBefore = host.Health.Current;
        outcome = TheirBlow();
        yield return Real(1f);
        // Its Static is discharged first; each bolt of the storm then lays one Static of its own.
        Judge("the enemy's blow calls the storm down before its time, a bolt for each of your three blows", !Cloud(TheGrudgeDuo.AshPlume) && host.Health.Current <= healthBefore - 30f,
            $"health lost {healthBefore - host.Health.Current:0}, Static {host.Afflictions.Stacks(AfflictionKind.Static)}");

        // Ash Plume's storm is five bolts at most (the owner's answer of 2026-11-20): seven blows on the cloud, five bolts
        // of 10, after its three Static discharged (21).
        yield return Clouded();
        for (int i = 0; i < 7; i++)
        {
            host.TakeHit(new Hit(1f, 0f, Vector3.forward));
            yield return Real(0.1f);
        }
        healthBefore = host.Health.Current;
        TheirBlow();
        yield return Real(1.2f);
        float lost = healthBefore - host.Health.Current;
        Judge("seven blows on the cloud call down five bolts, not seven", !Cloud(TheGrudgeDuo.AshPlume) && lost >= 60f && lost < 85f, $"health lost {lost:0} (five bolts and the discharge are about 71; seven would be about 91)");

        // The Exile Returns answers the Grudge's bolt alone: at the storm, a second enemy 2 m from the host is Cursed.
        upgrades.Give(Upgrade.ById(TheGrudgeDuo.TheExileReturns));
        upgrades.RefreshNow();
        yield return Clouded();
        var near = Instantiate(still.gameObject, Middle + Vector3.right * 2f, still.transform.rotation).GetComponent<TrainingDummy>();
        near.name = "Probe Neighbour";
        yield return Real(0.2f);
        Judge("before the bolt the neighbour carries no Curse", !near.Afflictions.Has(AfflictionKind.Cursed));
        TheirBlow();
        yield return Real(0.6f);
        Judge("The Exile Returns: the Grudge's bolt Curses everything within 3 m", near.Afflictions.Has(AfflictionKind.Cursed), $"neighbour Cursed {near.Afflictions.Stacks(AfflictionKind.Cursed)}");
        Destroy(near.gameObject);
        Finish();
    }

    private void Finish()
    {
        if (host != null) Destroy(host.gameObject);
        RunState.Unlocked.Remove(TheGrudgeDuo.Shape);
        RunState.Unlocked.Remove(TheGrudgeDuo.TheExileReturns);
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        if (defence != null) defence.ClearWards();
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
