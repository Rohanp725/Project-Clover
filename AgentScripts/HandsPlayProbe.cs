using System;
using System.Collections;
using System.IO;
using System.Linq;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The new hands (finger bones, 2026-11-22) in play in the Proving Ground: every weapon equipped in turn, its old clips
// played (idle, then a light attack with J), the clip named and the hands' grips read, and with time stopped mid-blow a
// close picture of the hands in the game's toon shader and ink plus the game view.
// Stills: review/shots/current/hands/play_<weapon>_close.png and play_<weapon>_view.png; lines "[Probe] OK/FAIL <weapon>".
//   bash AgentScripts/runprobe.sh ProvingGround HandsPlayProbe 80
public static class HandsPlayProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Hands Play Probe").AddComponent<HandsPlayProbeRunner>();
        return "probe started";
    }
}

public class HandsPlayProbeRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/hands";
    private int ok, failed;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private InputSettings.BackgroundBehavior backgroundWas;

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private static readonly (string tag, Func<Weapon> make)[] Kit =
    {
        ("katana", Weapons.Katana), ("fists", Weapons.Fists), ("gsword", Weapons.MassiveSword), ("scythe", Weapons.Scythe),
        ("spear", Weapons.GreatSpear), ("hammer", Weapons.GreatHammer), ("knife", Weapons.TwinKnives), ("bow", Weapons.GreatBow),
        ("sickle", Weapons.ChainSickle), ("arm", () => Weapons.Armament(Elements.Fire)),
    };

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        int presses = 0;
        while (GuidePage.Showing && presses++ < 20) yield return Tap(Key.Escape);
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && presses++ < 40) yield return Tap(Key.E);
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.4f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        var player = FindAnyObjectByType<PlayerController>();
        var combat = player.GetComponent<PlayerCombat>();
        var grip = player.GetComponentInChildren<HandGrip>();
        var animator = player.GetComponentInChildren<Animator>();
        var cameraObject = new GameObject("Hands Close Camera");
        var close = cameraObject.AddComponent<Camera>();
        close.fieldOfView = 30f;
        close.nearClipPlane = 0.02f;
        close.enabled = false;
        var rt = new RenderTexture(640, 640, 24, RenderTextureFormat.ARGB32);
        close.targetTexture = rt;
        Transform handR = FindDeep(player.transform, "hand.R"), handL = FindDeep(player.transform, "hand.L"), chest = FindDeep(player.transform, "chest");
        Say($"HandGrip {(grip != null ? "on " + grip.name + ", " + grip.Holds.Length + " holds" : "MISSING")}; fingers {(grip != null && grip.Fingers.All(f => f != null) ? "all found" : "missing")}");

        foreach (var (tag, make) in Kit)
        {
            combat.Equip(make());
            yield return Real(0.6f);
            string idle = ClipNow(animator);
            var idlePoses = $"{grip.PoseOf(0)}/{grip.PoseOf(1)}";
            yield return Tap(Key.J);
            yield return Real(0.16f);
            string attack = ClipNow(animator);
            Time.timeScale = 0f;
            yield return null;
            yield return null;
            var poses = $"{grip.PoseOf(0)}/{grip.PoseOf(1)}";
            float turn = Quaternion.Angle(Quaternion.identity, grip.TurnOf(0));
            // The weapon's handle in the right fist (or the left, for the bow): its part's grip point near the fist's middle.
            var held = grip.Holds.FirstOrDefault(h => h.Part != null && h.Part.activeInHierarchy && h.Pose != HandGrip.Pose.Open && !h.Part.name.StartsWith("arrow"));
            string seat = "no part held";
            if (held != null)
            {
                var hand = held.Hand == 0 ? handR : handL;
                seat = $"{held.Part.name} at {hand.InverseTransformPoint(held.Part.transform.position).ToString("F3")} in its hand";
            }
            var focus = (held != null && held.Hand == 1 ? handL : handR).TransformPoint(new Vector3(0f, 0.09f, 0f));
            Vector3 outward = focus - chest.position;
            outward.y = 0f;
            cameraObject.transform.position = focus + (outward.normalized + Vector3.up * 0.5f + Camera.main.transform.forward * -0.6f).normalized * 0.9f;
            cameraObject.transform.LookAt(focus);
            close.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var picture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            picture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            picture.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes($"{Folder}/play_{tag}_close.png", picture.EncodeToPNG());
            Destroy(picture);
            ScreenCapture.CaptureScreenshot($"{Folder}/play_{tag}_view.png");
            yield return null;
            yield return null;
            Time.timeScale = 1f;
            bool fists = tag == "fists" || tag == "arm";
            bool good = !string.IsNullOrEmpty(attack) && attack != idle && attack.Contains("attack")
                && (fists ? poses == "Fist/Fist" : held != null && poses.Contains(tag == "bow" ? "Bow" : "Closed"));
            if (good) ok++; else failed++;
            Say($"{(good ? "OK" : "FAIL")} {tag}: idle clip {idle} ({idlePoses}), after J {attack} ({poses}), right hand turned {turn:0.0} deg; {seat}");
            yield return Real(1.2f);
        }
        combat.Equip(Weapons.Katana());
        Destroy(cameraObject);
        rt.Release();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say($"tally: {ok} OK, {failed} FAIL");
        Say("done");
        Destroy(gameObject);
    }

    private static string ClipNow(Animator animator)
    {
        if (animator == null) return "(no animator)";
        string best = "";
        float weight = 0f;
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            float layerWeight = layer == 0 ? 1f : animator.GetLayerWeight(layer);
            foreach (var info in animator.GetCurrentAnimatorClipInfo(layer))
                if (info.clip != null && info.weight * layerWeight > weight) { weight = info.weight * layerWeight; best = info.clip.name; }
        }
        return best;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }
}
