using System.Collections;
using System.Linq;
using System.Text;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Plays chosen reaction looks on a copy of the Proving Ground's still dummy (player 3 m south, the blow going north)
// and logs every shown pool part's mesh and world bounds about the body's feet at five moments, for fixing where a
// part goes. Writes review/checks/reaction_poses.md. bash AgentScripts/runprobe.sh ProvingGround ReactionPoseDump 120
public static class ReactionPoseDump
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Pose Dump").AddComponent<ReactionPoseDumpRunner>();
        return "probe started";
    }
}

public class ReactionPoseDumpRunner : MonoBehaviour
{
    // Every designed look, then four composed ones; set Only to a few names to film just those.
    private static readonly string[] Composed = { "Thunder Gust", "Red Hot", "Windthrow", "Mock Suns" };
    private static readonly string[] Only = { };
    private static readonly float[] Times = { 0.05f, 0.15f, 0.3f, 0.45f, 0.7f, 1.0f };
    private const int CloseSize = 380;

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
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        var player = FindAnyObjectByType<PlayerController>();
        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).First(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        var host = Instantiate(still.gameObject, new Vector3(25f, 0f, 6f), still.transform.rotation);
        player.Teleport(host.transform.position + Vector3.back * 3f, Quaternion.identity);
        float wait = Time.unscaledTime + 1f;
        while (Time.unscaledTime < wait) yield return null;

        var report = new StringBuilder("# Reaction poses\n\nWorld bounds about the body's feet (x east, y up, z north; the player is 3 m south, the camera behind it).\n\n");
        var cam = Camera.main;
        report.AppendLine($"Camera at {cam.transform.position - host.transform.position:F2} looking {cam.transform.forward:F2}.\n");
        var looks = Enumerable.Range(0, AfflictionPairs.Count).Where(ReactionArt.IsDesigned).Select(AfflictionPairs.Name).Concat(Composed)
            .Where(name => Only.Length == 0 || Only.Contains(name)).ToArray();
        foreach (string look in looks)
        {
            int pair = Enumerable.Range(0, AfflictionPairs.Count).First(p => AfflictionPairs.Name(p) == look);
            var close = new Texture2D(CloseSize * Times.Length, CloseSize, TextureFormat.RGB24, false);
            int frameIndex = 0;
            float clear = Time.unscaledTime + 3f;
            while ((ReactionArt.PartsShown > 0 || ReactionArt.LampsShown > 0) && Time.unscaledTime < clear) yield return null;
            report.AppendLine($"## {look}\n");
            ReactionLook.Play(pair, host.transform.position, host.transform, Vector3.forward);
            float start = Time.time;
            foreach (float at in Times)
            {
                while (Time.time - start < at) yield return null;
                Time.timeScale = 0f;
                yield return new WaitForEndOfFrame();
                report.AppendLine($"- {at:0.00} s");
                foreach (var shape in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
                {
                    if (shape.name != "Shape" || !shape.enabled || shape.transform.parent == null || shape.transform.parent.name != "Reaction Part") continue;
                    var mesh = shape.GetComponent<MeshFilter>().sharedMesh;
                    var bounds = shape.bounds;
                    report.AppendLine($"  - {(mesh != null ? mesh.name : "?")}: centre {bounds.center - host.transform.position:F2}, size {bounds.size:F2}, frame scale {shape.transform.parent.localScale:F2}");
                }
                var frame = ScreenCapture.CaptureScreenshotAsTexture();
                Vector3 onScreen = cam.WorldToScreenPoint(host.transform.position + Vector3.up * 1.1f);
                float scaleX = frame.width / (float)Screen.width, scaleY = frame.height / (float)Screen.height;
                int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - CloseSize / 2, 0, Mathf.Max(0, frame.width - CloseSize));
                int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - CloseSize / 2 + 60, 0, Mathf.Max(0, frame.height - CloseSize));
                for (int y = 0; y < CloseSize; y++)
                    for (int x = 0; x < CloseSize; x++)
                        close.SetPixel(frameIndex * CloseSize + x, y, x == 0 ? Color.black : left + x < frame.width && bottom + y < frame.height ? frame.GetPixel(left + x, bottom + y) : Color.black);
                Destroy(frame);
                frameIndex++;
                Time.timeScale = 1f;
                yield return null;
            }
            close.Apply();
            System.IO.File.WriteAllBytes("review/shots/current/reactpose_" + look.ToLowerInvariant().Replace(' ', '_') + ".png", close.EncodeToPNG());
            Destroy(close);
            report.AppendLine();
        }
        System.IO.File.WriteAllText("review/checks/reaction_poses.md", report.ToString());
        Destroy(host);
        Debug.Log("[Probe] poses written");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
