using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe of the Katana's Way in the Proving Ground: the title guide is closed with the back key and time is
// shown to run (an open guide holds time at zero); the Katana is taken from the rack; the first trunk mark is bought
// with Memories through the wallet's rule; the probe checks the mark is in force and the wallet paid, then stops play.
// The game's own buy button still pays in Shards only, so the buy is the wallet's rule, not a chart press.
// Saved progress is kept out of it (runprobe.sh sandboxes the play; the probe refuses to pay if it is not).
//   bash AgentScripts/runprobe.sh ProvingGround WayProbe 40
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
    private int ok, wrong;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
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
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        interaction = player.GetComponent<PlayerInteraction>();
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
            combat.Equip(Weapons.ByName(Weapons.KatanaName));
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
        bool open = WayRules.Open(mark, Progress.Data.OwnedWeapons, PlaytestMode.On);
        bool canBuy = MarkRules.CanBuy(mark, RunState.Unlocked, wallet, open, PlaytestMode.On, Progress.Has, Progress.Data.GodsFelled);
        Judge("the wallet can buy it", canBuy, $"open {open}, {memoriesBefore} Memories held, playtest {PlaytestMode.On}");
        if (canBuy && wallet.Pay(mark))
        {
            RunState.Unlocked.Add(mark.Id);
            upgrades.RefreshNow();
        }
        yield return null;
        Judge("the wallet was charged its price", wallet.Memories == memoriesBefore - mark.Cost, $"{memoriesBefore} -> {wallet.Memories}, price {mark.Cost}");
        Judge("the mark is in force with the Katana held", upgrades.Has(mark.Id));

        Finish();
    }

    private void Finish()
    {
        Say($"summary: {ok} OK, {wrong} WRONG");
        Say("done");
        // The probe stops play itself, so nothing is left running with the world or a menu holding time.
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
