using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The owner's decision of 2026-11-20 (phase4_questions.md, Dark 1, rise-on-root), in the combat arena: the Rise is
// given by Dark's root, Curse; the cast names the root, so the Rise ends with it; and only the gift, A Life for a Life,
// buys the Rise back with a finisher on the killer. Every judged line ends OK or WRONG.
//   bash AgentScripts/runprobe.sh CombatTest RiseProbe 60
public static class RiseProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Rise Probe").AddComponent<RiseProbeRunner>();
        return "probe started";
    }
}

public class RiseProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private TrainingDummy killer;
    private int ok, wrong;
    private readonly Vector3 spot = new Vector3(5f, 0f, -2f), stand = new Vector3(2.5f, 0f, -1.9f);
    private Vector3 toward;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    // Dark held, only the marks named learned, whole health, the killer dummy before the player.
    private IEnumerator Ready(params string[] marks)
    {
        float patience = Time.unscaledTime + 8f;
        while ((defence.IsDead || player.FormHold != 0 || SpellWorks.Running > 0) && Time.unscaledTime < patience) yield return null;
        element.Take(ElementKit.ByName(Elements.Dark, false));
        upgrades.Forget();
        foreach (string id in marks) upgrades.Give(Upgrade.ById(id));
        upgrades.RefreshNow();
        defence.DamageTaken = 0f;
        defence.Restore();
        player.Teleport(stand, Quaternion.LookRotation(toward));
        killer.Afflictions.Clear();
        killer.Stagger.Reset();
        killer.Health.Fill();
        yield return new WaitForSeconds(0.4f);
    }

    private HitOutcome Kill()
    {
        defence.DamageTaken = 1f;
        var outcome = defence.TakeHit(new Hit(100000f, 0f, -toward, false, true, spot));
        defence.DamageTaken = 0f;
        return outcome;
    }

    private IEnumerator Finisher()
    {
        killer.Stagger.Add(100000f);
        yield return null;
        int finishers = combat.Finishers;
        yield return Tap(Key.K);
        float give = Time.unscaledTime + 2.5f;
        while (combat.Finishers == finishers && Time.unscaledTime < give) yield return null;
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        toward = (spot - stand).normalized;
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return new WaitForSeconds(0.3f);
        killer = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None).FirstOrDefault(dummy => dummy.GetComponent<DummyAttacker>() == null);
        Judge("a dummy stands in to be the killer", killer != null);
        if (killer == null) { Finish(); yield break; }
        killer.transform.position = spot;
        killer.Health.Maximum = 100000f;
        killer.RestoreAfter = 99999f;
        Physics.SyncTransforms();

        // The root and the gift: the finisher buys it back (first, since a Rise not bought back stays spent until a death).
        yield return Ready(DarkTree.Curse, DarkTree.LifeForALife);
        Kill();
        yield return new WaitForSeconds(0.3f);
        Judge("with the root and the gift it rises", !defence.IsDead && upgrades.Risen, $"dead {defence.IsDead}, risen {upgrades.Risen}");
        yield return Finisher();
        Judge("and the gift's finisher on the killer buys it back", !upgrades.Risen && defence.LastCallout == "A life for a life",
            $"risen {upgrades.Risen}, health {defence.Health.Current:0}, callout '{defence.LastCallout}'");

        // The root alone: it rises, and the finisher does not buy it back.
        yield return Ready(DarkTree.Curse);
        Judge("Dark's root is learned and in force, the gift is not", upgrades.Has(DarkTree.Curse) && !upgrades.Has(DarkTree.LifeForALife));
        Kill();
        yield return new WaitForSeconds(0.3f);
        Judge("with the root alone, the killing blow stands you up Risen", !defence.IsDead && upgrades.Risen,
            $"dead {defence.IsDead}, risen {upgrades.Risen}, health {defence.Health.Current:0} of {defence.Health.Maximum:0}, callout '{defence.LastCallout}'");
        float risen = defence.Health.Current;
        yield return Finisher();
        Judge("without the gift a finisher on the killer does not buy the Rise back", upgrades.Risen && defence.Health.Current <= risen + 0.5f,
            $"risen {upgrades.Risen}, health {risen:0} -> {defence.Health.Current:0}");
        // The cast names the root: forgetting it ends the Rise.
        RunState.Unlocked.Remove(DarkTree.Curse);
        upgrades.RefreshNow();
        yield return new WaitForSeconds(0.3f);
        Judge("the Rise's cast names the root: with the root gone the Rise ends", !upgrades.Risen, $"risen {upgrades.Risen}");

        // The gift alone gives no Rise: the killing blow kills.
        yield return Ready(DarkTree.LifeForALife);
        Kill();
        yield return new WaitForSeconds(0.3f);
        Judge("the gift alone gives no Rise", defence.IsDead && !upgrades.Risen, $"dead {defence.IsDead}, risen {upgrades.Risen}");
        float give = Time.unscaledTime + 10f;
        while (defence.IsDead && Time.unscaledTime < give) yield return null;
        yield return new WaitForSeconds(0.5f);
        Finish();
    }

    private void Finish()
    {
        if (upgrades != null) upgrades.Forget();
        if (element != null) element.Take(ElementKit.ByName(Elements.Fire, false));
        if (defence != null) defence.DamageTaken = 0f;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
