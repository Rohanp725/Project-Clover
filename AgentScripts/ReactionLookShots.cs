using System;
using System.Collections;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Each designed reaction look alone, for comparing with the look-dev frames: the still dummy's copy in the Proving
// Ground, the player 3 m south of it, every earlier look and element piece given 3 s to end, then the look played as a
// blow from the player would carry it (away = +Z) and a frame taken 0.15 s in, to review/shots/current/reactlook_<name>.png.
// bash AgentScripts/runprobe.sh ProvingGround ReactionLookShots 200
public static class ReactionLookShots
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Look Shots").AddComponent<ReactionLookShotsRunner>();
        return "probe started";
    }
}

public class ReactionLookShotsRunner : MonoBehaviour
{
    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

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

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = FindAnyObjectByType<PlayerController>();
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        if (still == null || !ReactionArt.Ready)
        {
            Say($"no still dummy ({still != null}) or no reaction art ({ReactionArt.Ready})");
            Say("done");
            Destroy(gameObject);
            yield break;
        }
        var host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Look Host";
        player.Teleport(Middle + Vector3.back * 3f, Quaternion.identity);
        yield return Real(1f);

        int shots = 0;
        for (int pair = 0; pair < AfflictionPairs.Count; pair++)
        {
            if (!ReactionArt.IsDesigned(pair)) continue;
            string name = AfflictionPairs.Name(pair).ToLowerInvariant().Replace(' ', '_');
            player.Teleport(Middle + Vector3.back * 3f, Quaternion.identity);
            yield return Real(3f);
            ReactionLook.Play(pair, host.transform.position, host.transform, Vector3.forward);
            yield return Real(0.15f);
            Time.timeScale = 0f;
            yield return null;
            ScreenCapture.CaptureScreenshot("review/shots/current/reactlook_" + name + ".png");
            yield return null;
            yield return null;
            Time.timeScale = 1f;
            shots++;
            Say($"{name}: parts {ReactionArt.PartsShown}, lamps {ReactionArt.LampsShown}");
        }
        Destroy(host.gameObject);
        Say($"shots {shots}");
        Say("done");
        Destroy(gameObject);
    }
}
