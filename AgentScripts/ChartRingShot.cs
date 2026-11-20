using System.Collections;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode: opens the chart in the Proving Ground, pulls back to the whole ring, says whether every drawn mark fits between
// the title bar and the keys' bar (UpgradeMenu.WideFits), and photographs it: review/shots/current/chart_ring_all.png.
// bash AgentScripts/runprobe.sh ProvingGround ChartRingShot 30
public static class ChartRingShot
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Chart Ring Shot").AddComponent<ChartRingShotRunner>();
        return "probe started";
    }
}

public class ChartRingShotRunner : MonoBehaviour
{
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

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
        DontDestroyOnLoad(gameObject);
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        var chart = UpgradeMenu.Instance;
        if (chart == null)
        {
            Debug.Log("[Probe] WRONG: no chart in the scene");
            Debug.Log("[Probe] done");
            Destroy(gameObject);
            yield break;
        }
        chart.Open();
        float wait = Time.unscaledTime + 0.6f;
        while (Time.unscaledTime < wait) yield return null;
        chart.ToggleWide();
        wait = Time.unscaledTime + 1.2f;
        while (Time.unscaledTime < wait) yield return null;
        Debug.Log($"[Probe] whole ring: {chart.MarkCount} marks, zoom {chart.Zoom:0.000}, every mark between the bars {(chart.WideFits() ? "OK" : "WRONG")}, screen {Screen.width}x{Screen.height}");
        ScreenCapture.CaptureScreenshot("review/shots/current/chart_ring_all.png");
        yield return null;
        yield return null;
        chart.ToggleWide();
        yield return Tap(Key.Space);
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
