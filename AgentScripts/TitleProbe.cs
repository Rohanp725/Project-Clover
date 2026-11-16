using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Play Mode probe: loads the open scene afresh and photographs its title card.
public static class TitleProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Title Probe").AddComponent<TitleProbeRunner>();
        return "probe started";
    }
}

public class TitleProbeRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
        yield return null;
        yield return new WaitForSecondsRealtime(1.9f);
        ScreenCapture.CaptureScreenshot($"review/shots/current/title_{scene.ToLower()}.png");
        yield return null;
        yield return null;
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
