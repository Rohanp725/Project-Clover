using System;
using System.Collections;
using System.Linq;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The Twin Knives in the player's hands in the Proving Ground, as the game plays them: equips them, closes the guide
// with Escape, proves time runs, then with time stopped takes a close picture of each hand at rest, at the first
// light cut's wind-up and in the heavy's dart, plus the game camera at rest.
// Frames: review/anim/knife_grip/game_<moment>_<hand>.png and game_<moment>_view.png.
//   bash AgentScripts/runprobe.sh ProvingGround KnifeGripProbe 40
public static class KnifeGripProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Knife Grip Probe").AddComponent<KnifeGripProbeRunner>();
        return "probe started";
    }
}

public class KnifeGripProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (player != null) player.Stamina.Fill();
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private void Close(string name, Transform hand)
    {
        var holder = new GameObject("Knife Grip Camera");
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 26f;
        camera.nearClipPlane = 0.05f;
        Vector3 grip = hand.position;
        Vector3 chest = player.transform.position + Vector3.up * 1.3f;
        Vector3 outward = grip - chest;
        outward.y = 0f;
        outward = outward.sqrMagnitude > 1e-4f ? outward.normalized : player.transform.right;
        holder.transform.position = grip + (outward + Vector3.up * 0.35f + player.transform.forward * 0.3f).normalized * 1.1f;
        holder.transform.LookAt(grip);
        var target = new RenderTexture(640, 640, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        System.IO.File.WriteAllBytes("review/anim/knife_grip/" + name + ".png", picture.EncodeToPNG());
        camera.targetTexture = null;
        Destroy(target);
        Destroy(picture);
        Destroy(holder);
    }

    private IEnumerator Pictures(string moment, Transform right, Transform left)
    {
        Time.timeScale = 0f;
        yield return null;
        Close("game_" + moment + "_right", right);
        Close("game_" + moment + "_left", left);
        ScreenCapture.CaptureScreenshot("review/anim/knife_grip/game_" + moment + "_view.png");
        yield return null;
        yield return null;
        Time.timeScale = 1f;
    }

    private IEnumerator Start()
    {
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        yield return Until(() => GuidePage.Showing, 4f);
        int guard = 0;
        while (GuidePage.Showing && guard++ < 20) yield return Tap(Key.Escape);
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.3f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        combat.Equip(Weapons.ByName(Weapons.TwinKnivesName));
        yield return Real(0.3f);
        player.Teleport(player.transform.position, Quaternion.LookRotation(Vector3.right, Vector3.up));
        yield return Real(0.9f);
        var right = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "knife_hand");
        var left = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "knife_hand_L");
        Say($"knives: right {(right != null && right.gameObject.activeInHierarchy ? "shown" : "MISSING")} local {right?.localEulerAngles}, left {(left != null && left.gameObject.activeInHierarchy ? "shown" : "MISSING")} local {left?.localEulerAngles}");
        if (right == null || left == null) { Say("done"); yield break; }
        var handR = right.parent;
        var handL = left.parent;
        yield return Pictures("idle", handR, handL);

        yield return Real(0.4f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current != null && combat.Combo.StepProgress * combat.Combo.Current.Total >= combat.Combo.Current.Windup * 0.9f, 2f);
        yield return Pictures("light", handR, handL);
        yield return Until(() => !combat.IsAttacking, 3f);
        yield return Real(0.6f);

        // The heavy: held past its charge, let go, caught in the dart.
        Keys(Key.K);
        yield return Real(0.25f);
        Keys();
        yield return Until(() => combat.Combo.Current != null && combat.Combo.Current.Heavy && combat.Combo.StepProgress * combat.Combo.Current.Total >= combat.Combo.Current.Windup, 2f);
        yield return Pictures("heavy", handR, handL);
        Say($"heavy step: {(combat.Combo.Current != null ? combat.Combo.Current.Name : "none")}");
        yield return Until(() => !combat.IsAttacking, 3f);

        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say("done");
        Destroy(gameObject);
    }
}
