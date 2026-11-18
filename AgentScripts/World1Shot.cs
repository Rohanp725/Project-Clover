using System.Collections;
using System.Linq;
using Clover.Player;
using Clover.World;
using UnityEngine;

// Play Mode probe for the Fire land's world pass: puts the player at each place listed in AgentScripts/world1_shots.txt
// ("name x y z" a line; no file or an empty one means one frame where the player stands) with everyone calmed, and
// takes a frame of each to review/shots/current/world1_<name>.png.
public static class World1Shot
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("World1 Shot").AddComponent<World1ShotRunner>();
        return "probe started";
    }
}

public class World1ShotRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var director = FindAnyObjectByType<AreaDirector>();
        string list = System.IO.File.Exists("AgentScripts/world1_shots.txt") ? System.IO.File.ReadAllText("AgentScripts/world1_shots.txt") : "";
        var lines = list.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
        // Any reading box the land opens on waking is read through first.
        yield return new WaitForSeconds(1.5f);
        var box = Clover.UI.ReadingBox.Instance;
        for (int i = 0; i < 30 && box != null && box.IsOpen; i++)
        {
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.E));
            yield return null;
            yield return null;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState());
            yield return new WaitForSecondsRealtime(0.08f);
        }
        if (lines.Length == 0) lines = new[] { "start" };
        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            if (parts.Length >= 4 && player != null)
                player.Teleport(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]) + 0.1f, float.Parse(parts[3])), Quaternion.identity);
            if (director != null)
                foreach (var enemy in director.Enemies)
                    if (enemy != null && !enemy.IsDead) enemy.Restore();
            yield return new WaitForSeconds(1.2f);
            ScreenCapture.CaptureScreenshot("review/shots/current/world1_" + parts[0] + ".png");
            yield return null;
            yield return null;
            Debug.Log("[Probe] shot " + parts[0]);
        }
        Debug.Log("[Probe] done");
    }
}
