using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the Katana's finisher, run in the Castle: staggers a soldier, finishes it, and takes a picture at
// the wind-up, the frame the blow lands and a quarter of a second into the slowed follow-through
// (review/shots/current/finisher_*.png). It also reports how far the camera stands off before, during and after.
//   unity command script run -- --file AgentScripts/FinisherProbe.cs --entry FinisherProbe.Run
public static class FinisherProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Finisher Probe").AddComponent<FinisherProbeRunner>();
        return "probe started";
    }
}

public class FinisherProbeRunner : MonoBehaviour
{
    private PlayerDefence defence;

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot("review/shots/current/finisher_" + name + ".png");
        yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        var director = FindAnyObjectByType<AreaDirector>();
        var view = FindAnyObjectByType<TopDownCamera>();
        yield return new WaitForSeconds(0.5f);
        combat.Equip(Weapons.ByName(Weapons.KatanaName));

        Enemy enemy = null;
        foreach (var candidate in director.Enemies)
            if (candidate.Kind.Name == EnemyKind.SoldierName) { enemy = candidate; break; }
        if (enemy == null) { Say("done: no soldier"); yield break; }

        Vector3 at = enemy.transform.position + enemy.transform.forward * 2.5f;
        player.Teleport(at, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
        yield return new WaitForSeconds(0.6f);
        float usual = Vector3.Distance(view.transform.position, player.transform.position);
        enemy.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        yield return null;
        Say($"staged: staggered {enemy.IsStaggered} weapon {combat.Weapon.Name} camera off {usual:0.00} framing {view.HasFraming}");

        int finishers = combat.Finishers;
        Keys(Key.K);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => combat.IsFinishing, 1f);
        yield return new WaitForSeconds(0.2f);
        Say($"wind-up: finishing {combat.IsFinishing} gap {Vector3.Distance(player.transform.position, enemy.transform.position):0.00}");
        yield return Shot("windup");
        yield return Until(() => combat.Finishers > finishers, 1.5f);
        Say($"hit: finishers +{combat.Finishers - finishers} time scale {Time.timeScale:0.00} gap {Vector3.Distance(player.transform.position, enemy.transform.position):0.00}");
        yield return Shot("hit");
        yield return new WaitForSecondsRealtime(0.45f);
        Say($"held: time scale {Time.timeScale:0.00} camera off {Vector3.Distance(view.transform.position, player.transform.position):0.00} (usual {usual:0.00})");
        yield return Shot("held");
        yield return new WaitForSecondsRealtime(0.45f);
        Say($"recovering: time scale {Time.timeScale:0.00} camera off {Vector3.Distance(view.transform.position, player.transform.position):0.00}");
        yield return Shot("flick");
        yield return new WaitForSecondsRealtime(1.5f);
        Say($"after: camera off {Vector3.Distance(view.transform.position, player.transform.position):0.00} (usual {usual:0.00}) framing {view.HasFraming}");
        Say("done: finisher");
        Destroy(gameObject);
    }
}
