using System.Collections;
using Clover.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

// Play Mode probe that walks the open scene's places of interest and photographs each to review/shots/current/tour_*.png.
//   unity command script run -- --file AgentScripts/TourProbe.cs --entry TourProbe.Run
public static class TourProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Tour Probe").AddComponent<TourProbeRunner>();
        return "probe started";
    }
}

public class TourProbeRunner : MonoBehaviour
{
    private PlayerDefence defence;

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        string scene = SceneManager.GetActiveScene().name;
        Vector3[] stops = scene == "Castle"
            ? new[] { new Vector3(0f, 0.1f, 6f), new Vector3(0f, 0.1f, 10f), new Vector3(-2f, 0.1f, 15f), new Vector3(0f, 0.1f, 24f), new Vector3(8f, 0.1f, 33f), new Vector3(9f, 0.1f, 42f), new Vector3(14f, 0.1f, 44f), new Vector3(-13f, 0.1f, 38f), new Vector3(-14f, 0.1f, 50f), new Vector3(-8f, 0.1f, 58f), new Vector3(0f, 2.1f, 66f), new Vector3(0f, 2.1f, 71f), new Vector3(-10f, 2.1f, 70f), new Vector3(14f, 2.1f, 80f), new Vector3(0f, 2.1f, 84f), new Vector3(-13f, 2.1f, 90f), new Vector3(0f, 4.1f, 104f), new Vector3(12f, 4.1f, 110f), new Vector3(0f, 4.1f, 120f), new Vector3(12f, 4.1f, 121f), new Vector3(-12f, 4.1f, 120f), new Vector3(-14f, 4.1f, 106f) }
            : scene == "Summit" ? new[] { new Vector3(0f, 0.1f, -12f), new Vector3(0f, 0.1f, 0f), new Vector3(6f, 0.1f, 6f), new Vector3(0f, 0.1f, 13f), new Vector3(13f, 0.1f, 0f), new Vector3(0f, 0.1f, -14.5f), new Vector3(-13f, 0.1f, 0f), new Vector3(-9f, 0.1f, -9f), new Vector3(9f, 0.1f, 9f) }
            : scene == "Hub" ? new[] { player.transform.position, new Vector3(0f, 0.1f, 5.5f), new Vector3(-4.5f, 0.1f, -2.5f), new Vector3(4.5f, 0.1f, -2.5f), new Vector3(0f, 0.1f, -8.5f), new Vector3(-7.5f, 0.1f, 3f), new Vector3(7.5f, 0.1f, 3f) }
            : new[] { player.transform.position };
        yield return new WaitForSecondsRealtime(1.6f);
        ScreenCapture.CaptureScreenshot($"review/shots/current/tour_{scene.ToLower()}_title.png");
        yield return null;
        for (int i = 0; i < stops.Length; i++)
        {
            player.Teleport(stops[i], Quaternion.identity);
            yield return new WaitForSeconds(1.2f);
            ScreenCapture.CaptureScreenshot($"review/shots/current/tour_{scene.ToLower()}_{i}.png");
            yield return null;
            yield return null;
        }
        Debug.Log("[Probe] camera " + Camera.main.transform.eulerAngles + " at " + Camera.main.transform.position);
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
