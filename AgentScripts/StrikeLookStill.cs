using System.Collections;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Stills of the redrawn infused strikes' looks on one struck body, with nothing else drawn on it: a Temple Soldier stood
// on the open floor in front of the player, each strike's "@b" look played on it (and its look about the player at the
// player's feet), and the game view grabbed at set ages. Pictures to review/gifs/swings/stills/<element>_<age>.png.
// bash AgentScripts/runprobe.sh ProvingGround StrikeLookStill 80
public static class StrikeLookStill
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Strike Look Still").AddComponent<StrikeLookStillRunner>();
        return "probe started";
    }
}

public class StrikeLookStillRunner : MonoBehaviour
{
    private static readonly Vector3 Middle = new Vector3(28f, 0.05f, 0f);
    private static readonly string[] Elements = { "earth", "metal", "light", "dark", "poison" };
    private static readonly float[] Ages = { 0.15f, 0.35f, 0.6f, 0.9f };

    private static void Say(string text) => Debug.Log("[Probe] " + text);
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

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        var settingsWas = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        yield return Real(1f);
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 5) yield return Tap(Key.Escape);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (!ElementArt.On) ElementArt.Flip();
        var player = FindAnyObjectByType<PlayerController>();
        Enemy soldier = null;
        foreach (var pen in FindObjectsByType<EnemyPen>(FindObjectsInactive.Include))
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            foreach (var enemy in pen.Enemies)
            {
                if (enemy == null) continue;
                if (soldier == null && enemy.Kind.Name == EnemyKind.SoldierName) soldier = enemy;
                else enemy.gameObject.SetActive(false);
            }
        }
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace) canvas.enabled = false;
        foreach (var text in FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude)) text.gameObject.SetActive(false);
        var reticle = player.GetComponent<TargetReticle>();
        if (reticle != null) reticle.enabled = false;
        player.Teleport(Middle + Vector3.back * 2f, Quaternion.identity);
        yield return Real(1f);
        if (soldier == null)
        {
            Say("WRONG: no Temple Soldier");
            Say("done");
            yield break;
        }
        System.IO.Directory.CreateDirectory("review/gifs/swings/stills");
        foreach (string element in Elements)
        {
            soldier.transform.SetPositionAndRotation(Middle, Quaternion.Euler(0f, 180f, 0f));
            yield return Real(0.6f);
            int onBody = ElementLooks.Find("e3_strike_" + element + "@b"), about = ElementLooks.Find("e3_strike_" + element);
            if (about >= 0) ElementLooks.PlayAt(about, player.transform.position, 0f);
            if (onBody >= 0) ElementLooks.Play(onBody, soldier.transform, 0f);
            float start = Time.time;
            foreach (float age in Ages)
            {
                while (Time.time - start < age) yield return null;
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot($"review/gifs/swings/stills/{element}_{age:0.00}.png");
                yield return null;
            }
            Say($"{element}: about {about}, on body {onBody}");
            yield return Real(1.2f);
        }
        InputSystem.settings.backgroundBehavior = settingsWas;
        Say("done");
    }
}
