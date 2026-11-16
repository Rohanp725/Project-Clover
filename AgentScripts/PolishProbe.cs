using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;

// Play Mode probe for the polish pass, in the Castle: a bolt landing on the player, an enemy's bars where the player's own
// bars are on the screen, and what the see-through fade does to a low planter and to the fighters beside it.
//   unity command script run -- --file AgentScripts/PolishProbe.cs --entry PolishProbe.Run
public static class PolishProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Polish Probe").AddComponent<PolishProbeRunner>();
        return "probe started";
    }
}

public class PolishProbeRunner : MonoBehaviour
{
    private PlayerDefence defence;

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private static int Thinned()
    {
        int count = 0;
        foreach (var piece in FindObjectsByType<Fadeable>(FindObjectsSortMode.None))
            if (piece.Fade < 0.99f) count++;
        return count;
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var view = Camera.main;

        // 1: a bolt on the player, photographed while the line is at its widest.
        player.Teleport(new Vector3(0f, 0.1f, 10f), Quaternion.identity);
        yield return new WaitForSeconds(0.8f);
        LightningBolts.Strike(player.transform.position, 0.5f);
        yield return null;
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/pol_bolt_player.png");
        yield return new WaitForSeconds(0.5f);

        // 2: the planter of the shrine's tree between the camera and the player.
        player.Teleport(new Vector3(-10f, 2.1f, 74.3f), Quaternion.identity);
        yield return new WaitForSeconds(0.8f);
        Say($"2 behind the planter: pieces thinned {Thinned()}");
        ScreenCapture.CaptureScreenshot("review/shots/current/pol_fade_planter.png");
        yield return new WaitForSeconds(0.3f);

        // 3: the Gate Captain where the player's own bars are on the screen.
        Enemy captain = null;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (enemy.Kind.IsElite) captain = enemy;
        if (captain != null)
        {
            player.Teleport(captain.transform.position + new Vector3(6.8f, 0f, -4.6f), Quaternion.identity);
            float end = Time.unscaledTime + 4f;
            EnemyBars bars = null;
            while (Time.unscaledTime < end && bars == null)
            {
                bars = captain.GetComponentInChildren<EnemyBars>();
                // He comes for the player; keep him where the corner of the screen is.
                player.Teleport(captain.transform.position + new Vector3(6.8f, 0f, -4.6f), Quaternion.identity);
                yield return null;
            }
            for (float until = Time.unscaledTime + 1.5f; Time.unscaledTime < until;)
            {
                player.Teleport(captain.transform.position + new Vector3(6.8f, 0f, -4.6f), Quaternion.identity);
                yield return null;
            }
            Vector3 seen = bars != null ? view.WorldToViewportPoint(bars.transform.position) : Vector3.zero;
            Vector3 body = view.WorldToViewportPoint(captain.transform.position + Vector3.up * 2f);
            Say($"3 captain in the corner: bars showing {bars != null}, bars on screen at {seen.x:0.00},{seen.y:0.00}, his body at {body.x:0.00},{body.y:0.00}; pieces thinned {Thinned()}");
            ScreenCapture.CaptureScreenshot("review/shots/current/pol_bars_corner.png");
            yield return new WaitForSeconds(0.3f);

            // 4: the captain straight north of the player, his name over his own body.
            for (float until = Time.unscaledTime + 1.5f; Time.unscaledTime < until;)
            {
                player.Teleport(captain.transform.position + new Vector3(0f, 0f, -3.4f), Quaternion.identity);
                yield return null;
            }
            ScreenCapture.CaptureScreenshot("review/shots/current/pol_name_north.png");
            yield return new WaitForSeconds(0.3f);
        }
        Say("done");
    }
}
