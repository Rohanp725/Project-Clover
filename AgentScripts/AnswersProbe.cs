using System;
using System.Collections;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Single stills of the owner's answers of 2026-11-22, in the Proving Ground (the day rule: stills only): (a) with Lightning
// the Scythe strikes in place (how far the player moved is logged for all four shape weapons, the Katana's 5 m lunge
// beside them); (b) a Fire infused strike's Burn lands at once but the body's flames wait 0.4 s (stills at 0.15 s and
// 0.75 s after the blow); (c) a plain Katana blow lands with no white flash (still at the flash). The guide closed with
// Escape and time shown to run first. Stills review/shots/current/answers/<name>.png.
//   bash AgentScripts/runprobe.sh ProvingGround AnswersProbe 120
public static class AnswersProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Answers Probe").AddComponent<AnswersProbeRunner>();
        return "probe started";
    }
}

public class AnswersProbeRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/answers";
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerSpells spells;
    private PlayerElement element;
    private PlayerDefence defence;
    private Enemy target;
    private Vector3 spot;
    private bool hold;
    private int ok, wrong;

    private static void Keys(params Key[] down) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(down));
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

    private IEnumerator Game(float seconds)
    {
        float end = Time.time + seconds;
        float give = Time.unscaledTime + seconds + 3f;
        while (Time.time < end && Time.unscaledTime < give) yield return null;
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

    private IEnumerator Still(string name)
    {
        float scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot($"{Folder}/{name}.png");
        yield return null;
        yield return null;
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    private void LateUpdate()
    {
        if (!hold || target == null || player == null) return;
        Vector3 to = player.transform.position - spot;
        to.y = 0f;
        target.transform.SetPositionAndRotation(spot, to.sqrMagnitude > 0.01f ? Quaternion.LookRotation(to) : Quaternion.identity);
    }

    private IEnumerator Infused()
    {
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        player.Stamina.Fill();
        defence.UntouchableUntil = Time.time + 6f;
        int strikes = combat.InfusedStrikes;
        for (int attempt = 0; attempt < 3 && combat.InfusedStrikes == strikes && !combat.InfusedNow; attempt++)
        {
            Keys(Key.LeftCtrl);
            yield return null;
            yield return null;
            Keys(Key.LeftCtrl, Key.J);
            yield return null;
            yield return null;
            Keys(Key.LeftCtrl);
            yield return null;
            Keys();
            yield return Until(() => combat.InfusedNow, 1f);
        }
    }

    private void Fresh(EnemyPen castle, int soldier)
    {
        castle.Release(new[] { soldier });
        target.Health.Maximum = 100000f;
        target.Health.Fill();
        target.Stagger.Maximum = 100000f;
    }

    private IEnumerator Start()
    {
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        spells = player.GetComponent<PlayerSpells>();
        element = player.GetComponent<PlayerElement>();
        defence = player.GetComponent<PlayerDefence>();
        yield return Until(() => GuidePage.Showing, 5f);
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        float before = Time.time;
        yield return Real(0.5f);
        Judge("the guide is closed and time runs", !GuidePage.Showing && Time.time > before, $"game time {before:0.00} -> {Time.time:0.00}");

        var castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Exclude).First(pen => pen.Land == "The castle");
        int soldier = Array.FindIndex(castle.Enemies, enemy => enemy != null && enemy.Kind.Name == EnemyKind.SoldierName);
        castle.Release(new[] { soldier });
        target = castle.Enemies[soldier];
        yield return Real(0.4f);
        Vector3 stand = player.transform.position;
        spot = stand + Vector3.forward * 1.8f;
        hold = true;

        // (a) Lightning: the shape weapons strike where they stand; the Katana lunges.
        foreach (var weapon in new[] { Weapons.ScytheName, Weapons.GreatSpearName, Weapons.TwinKnivesName, Weapons.ChainSickleName, Weapons.KatanaName })
        {
            combat.Arm(weapon);
            element.Take(ElementKit.ByName(Elements.Lightning, false));
            yield return Real(0.5f);
            Fresh(castle, soldier);
            player.Teleport(stand, Quaternion.identity);
            yield return Real(0.4f);
            Vector3 from = player.transform.position;
            yield return Infused();
            bool began = combat.InfusedNow;
            yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1.5f);
            if (weapon == Weapons.ScytheName)
            {
                yield return Game(0.08f);
                yield return Still("lightning_scythe_in_place");
            }
            yield return Until(() => !combat.IsAttacking, 3f);
            float moved = Vector3.Distance(Flat(from), Flat(player.transform.position));
            bool arc = weapon == Weapons.KatanaName;
            float own = combat.Weapon.CarrierStep.Travel + combat.Weapon.CarrierStep.Lunge;
            Judge($"Lightning {weapon}: {(arc ? "lunges" : "strikes in place")}", began && (arc ? moved > 3f : moved < own + 0.6f), $"moved {moved:0.00} m, its own carrier {own:0.00} m");
            yield return Real(0.6f);
        }

        // (b) Fire: the Burn at once, the body's flames 0.4 s later.
        combat.Arm(Weapons.KatanaName);
        element.Take(ElementKit.ByName(Elements.Fire, false));
        yield return Real(0.6f);
        Fresh(castle, soldier);
        player.Teleport(stand, Quaternion.identity);
        yield return Real(0.4f);
        int landed = combat.BlowsLanded;
        yield return Infused();
        yield return Until(() => target.Afflictions.Stacks(AfflictionKind.Burn) > 0, 2f);
        float struckAt = Time.time;
        float health = target.Health.Current;
        yield return Game(0.15f);
        bool waiting = BodyFlames.Waiting(target.transform);
        int burn = target.Afflictions.Stacks(AfflictionKind.Burn);
        yield return Still("fire_flames_wait");
        yield return Game(0.6f);
        bool waitingLater = BodyFlames.Waiting(target.transform);
        yield return Still("fire_flames_caught");
        Judge("Fire: Burn on at once, flames held back, then caught", burn > 0 && waiting && !waitingLater,
            $"burn {burn}, waiting at 0.15 s {waiting}, at 0.75 s {waitingLater}, health {health:0} -> {target.Health.Current:0} by {Time.time - struckAt:0.00} s");
        yield return Until(() => !combat.IsAttacking, 3f);
        yield return Real(3f);

        // (c) A plain Katana blow: the flash in the weapon's colour.
        element.Attune("", "");
        yield return Real(0.4f);
        Fresh(castle, soldier);
        player.Teleport(stand, Quaternion.identity);
        yield return Real(0.4f);
        landed = combat.BlowsLanded;
        yield return Tap(Key.J);
        yield return Until(() => combat.BlowsLanded > landed, 1.5f);
        yield return null;
        yield return Still("katana_plain_flash");
        Judge("plain Katana blow landed", combat.BlowsLanded > landed, $"element {(element.HasElement ? element.Info.Name : "none")}, tint {combat.WeaponCutTint}");

        hold = false;
        castle.Clear();
        Say($"summary: {ok} OK, {wrong} WRONG");
        Say("done");
        UnityEditor.EditorApplication.ExitPlaymode();
    }

    private static Vector3 Flat(Vector3 at) => new Vector3(at.x, 0f, at.z);
}
