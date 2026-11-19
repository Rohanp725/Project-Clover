using System;
using System.Collections;
using System.Linq;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The plain weapons of 2026-11-19 in the player's hands, in the Proving Ground: for the Massive Sword (with its parrying hooks), the Katana, Great Hammer, Twin
// Knives, Chain Sickle and Great Spear in turn it equips the weapon, stands the player in the open facing east, and takes three
// pictures with time stopped: the game camera at rest (the Katana held in its guard, since it is sheathed at rest),
// the game camera at the light attack's wind-up, and a close camera of its own 3 m off the player's right-front.
// It also writes where the Katana's old stick and its new model lie in the hand socket, so their axes can be compared.
// Frames: review/shots/current/weapons_<name>.png, weapons_<name>_swing.png, weapons_<name>_close.png.
//   bash AgentScripts/runprobe.sh ProvingGround WeaponLookProbe 60
public static class WeaponLookProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Weapon Look Probe").AddComponent<WeaponLookProbeRunner>();
        return "probe started";
    }
}

public class WeaponLookProbeRunner : MonoBehaviour
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

    private IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    // A camera of its own, 3 m off the player's right-front at chest height, rendered once and thrown away.
    private void Close(string name)
    {
        var holder = new GameObject("Weapon Look Camera");
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 38f;
        Vector3 centre = player.transform.position + Vector3.up * 1.05f;
        holder.transform.position = centre + player.transform.rotation * new Vector3(1.9f, 0.6f, 2.2f);
        holder.transform.LookAt(centre);
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        System.IO.File.WriteAllBytes("review/shots/current/" + name + ".png", picture.EncodeToPNG());
        camera.targetTexture = null;
        Destroy(target);
        Destroy(picture);
        Destroy(holder);
    }

    // Where a part's mesh lies in the socket's axes: the smallest and largest corner of its bounds.
    public static string InSocket(Transform socket, Transform part)
    {
        var filter = part.GetComponentInChildren<MeshFilter>(true);
        if (filter == null || filter.sharedMesh == null) return "no mesh";
        Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
        var b = filter.sharedMesh.bounds;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            Vector3 local = socket.InverseTransformPoint(filter.transform.TransformPoint(corner));
            min = Vector3.Min(min, local);
            max = Vector3.Max(max, local);
        }
        Vector3 e = b.extents;
        Vector3 longest = e.x >= e.y && e.x >= e.z ? new Vector3(e.x, 0f, 0f) : e.y >= e.z ? new Vector3(0f, e.y, 0f) : new Vector3(0f, 0f, e.z);
        Vector3 one = socket.InverseTransformPoint(filter.transform.TransformPoint(b.center - longest));
        Vector3 other = socket.InverseTransformPoint(filter.transform.TransformPoint(b.center + longest));
        Vector3 near = one.sqrMagnitude < other.sqrMagnitude ? one : other, far = one.sqrMagnitude < other.sqrMagnitude ? other : one;
        return $"min {min.ToString("F2")} max {max.ToString("F2")}; long axis from {near.ToString("F2")} to {far.ToString("F2")}, direction {(far - near).normalized.ToString("F2")}";
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
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        var socket = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == WeaponRack.HandRight);
        var drawn = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "katana_drawn");
        var stick = player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "katana_drawn_stick");
        Say($"hand socket {(socket != null ? "found" : "MISSING")}; katana_drawn under {(drawn != null ? drawn.parent.name : "none")}, stick {(stick != null ? (stick.gameObject.activeSelf ? "SHOWN" : "hidden") : "none")}");
        if (socket != null && stick != null) Say($"old stick in the socket: {InSocket(socket, stick)}");
        if (socket != null && drawn != null) Say($"new katana in the socket: {InSocket(socket, drawn)}");

        Vector3 at = player.transform.position;
        foreach (var (name, tag) in new[] { (Weapons.MassiveSwordName, "gsword_hooks"), (Weapons.KatanaName, "katana"), (Weapons.GreatHammerName, "hammer"), (Weapons.TwinKnivesName, "knives"), (Weapons.ChainSickleName, "sickle"), (Weapons.GreatSpearName, "spear") })
        {
            combat.Equip(Weapons.ByName(name));
            yield return Real(0.3f);
            player.Teleport(at, Quaternion.LookRotation(Vector3.right, Vector3.up));
            yield return Real(0.8f);
            bool sword = combat.Weapon.UsesSword;
            if (sword)
            {
                Keys(Key.L);
                yield return Real(0.7f);
            }
            var shown = player.GetComponent<PlayerAnimator>().ShownWeapon;
            Say($"{tag}: holding {combat.Weapon.Name}, shown part {(shown != null ? shown.name : "none")} active {(shown != null && shown.activeInHierarchy)}");
            Time.timeScale = 0f;
            yield return Shot("weapons_" + tag);
            Close("weapons_" + tag + "_close");
            Time.timeScale = 1f;
            if (sword) Keys();
            yield return Real(0.5f);
            yield return Tap(Key.J);
            yield return Until(() => combat.Combo.Current != null && combat.Combo.StepProgress * combat.Combo.Current.Total >= combat.Combo.Current.Windup * 0.8f, 2f);
            Time.timeScale = 0f;
            yield return Shot("weapons_" + tag + "_swing");
            Close("weapons_" + tag + "_swing_close");
            Time.timeScale = 1f;
            yield return Until(() => !combat.IsAttacking, 3f);
            yield return Real(0.6f);
        }

        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say("done");
        Destroy(gameObject);
    }
}
