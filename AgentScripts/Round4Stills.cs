using System.Collections;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Single stills of the round-4 Unity fixes that show outside a reaction (the day rule: stills only, films at night), in the
// Proving Ground: the finish mark over a staggered soldier (the cross alone, pulsing over its head), the cut line of a
// plain Katana blow (the weapon's slash colour, not white), Flame Wave's burning ground on its scorch, Miasma with no pool
// under its cloud, and Chill with no crystals. The guide closed with Escape and time shown to run first. Stills
// review/shots/current/round4/<name>.png.
//   bash AgentScripts/runprobe.sh ProvingGround Round4Stills 60
public static class Round4Stills
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Round 4 Stills").AddComponent<Round4StillsRunner>();
        return "probe started";
    }
}

public class Round4StillsRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/round4";
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

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

    private IEnumerator Start()
    {
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        Directory.CreateDirectory(Folder);
        var player = FindAnyObjectByType<PlayerController>();
        var combat = player.GetComponent<PlayerCombat>();
        var element = player.GetComponent<PlayerElement>();
        var spells = player.GetComponent<PlayerSpells>();
        var defence = player.GetComponent<PlayerDefence>();
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        float before = Time.time;
        yield return Real(0.5f);
        Say($"guide closed {!GuidePage.Showing}, time runs {Time.time > before}");

        var castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Exclude).First(pen => pen.Land == "The castle");
        int soldier = System.Array.FindIndex(castle.Enemies, enemy => enemy != null && enemy.Kind.Name == EnemyKind.SoldierName);
        castle.Release(new[] { soldier });
        var target = castle.Enemies[soldier];
        yield return Real(0.4f);
        Vector3 spot = target.transform.position;
        void Face(float metres)
        {
            Vector3 at = target.transform.position + Vector3.back * metres;
            player.Teleport(new Vector3(at.x, 0.05f, at.z), Quaternion.identity);
        }
        combat.Arm(Weapons.KatanaName);
        target.Health.Maximum = 100000f;
        target.Health.Fill();

        // The finish mark.
        Face(2.4f);
        yield return Real(0.4f);
        defence.UntouchableUntil = Time.time + 30f;
        target.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        yield return Real(0.5f);
        yield return Still("finish_mark");
        Say($"finish mark: staggered {target.IsStaggered}");
        yield return Real(2.5f);

        // A plain cut, no element carried.
        castle.Release(new[] { soldier });
        yield return Real(0.3f);
        target.Health.Maximum = 100000f;
        target.Health.Fill();
        Face(1.6f);
        yield return Real(0.3f);
        int landed = combat.BlowsLanded;
        yield return Tap(Key.J);
        float until = Time.unscaledTime + 1f;
        while (combat.BlowsLanded == landed && Time.unscaledTime < until) yield return null;
        yield return null;
        yield return Still("cut_plain");
        Say($"cut: landed {combat.BlowsLanded - landed}, element {(element.HasElement ? element.Info.Name : "none")}");
        yield return Real(1f);

        // Flame Wave's burning ground; Miasma's cloud; Chill.
        foreach (var (name, wait) in new[] { (Elements.Fire, 1.2f), (Elements.Poison, 1.0f) })
        {
            element.Take(ElementKit.ByName(name, false));
            yield return Real(0.6f);
            castle.Release(new[] { soldier });
            target.Health.Maximum = 100000f;
            target.Health.Fill();
            Face(3f);
            yield return Real(0.4f);
            spells.Cooldowns.ResetAll();
            bool cast = element.CastBase(name);
            yield return Real(wait);
            yield return Still($"spell_{name.ToLowerInvariant()}");
            Say($"{name}: cast {cast}");
            yield return Real(3f);
        }
        castle.Release(new[] { soldier });
        yield return Real(0.3f);
        target.Afflictions.Lay(AfflictionKind.Chill, 3, 0f, 1f, LaySource.Spell, target);
        Face(2.4f);
        yield return Real(0.8f);
        yield return Still("chill");
        castle.Clear();
        Say("done");
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
