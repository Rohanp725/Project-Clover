using System;
using System.Collections;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for the outfits: says what the player wears and how it went on, then photographs it standing,
// running, rolling, attacking and close beside an enemy, with time stopped for each picture
// (review/shots/current/outfit_<scene>_<pose>.png). In CombatTest it puts on every outfit in turn. In SunCity it then
// goes to the Hub and checks that the Fire outfit stays on there. Run through runprobe.sh with the Game View at 1280x720.
public static class OutfitProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        var runner = new GameObject("Outfit Probe").AddComponent<OutfitProbeRunner>();
        UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
        return "probe started";
    }
}

public class OutfitProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerOutfit outfit;
    private TopDownCamera follow;
    private string scene;

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null) defence.Health.Fill();
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    // Reads whatever the voice has put in the reading box to its end, so the buttons are the player's again.
    private IEnumerator Hear()
    {
        yield return new WaitForSeconds(0.5f);
        var box = Clover.UI.ReadingBox.Instance;
        for (int i = 0; i < 30 && box != null && box.IsOpen; i++)
        {
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return new WaitForSeconds(0.05f);
        }
        Say($"reading box open {box != null && box.IsOpen}");
    }

    private void Find()
    {
        scene = SceneManager.GetActiveScene().name;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        outfit = player.GetComponent<PlayerOutfit>();
        follow = FindAnyObjectByType<TopDownCamera>();
    }

    private string Describe()
    {
        var body = player.GetComponentInChildren<SkinnedMeshRenderer>();
        return $"{scene}: wears '{outfit?.Worn}' rebound {outfit?.Rebound} mesh {body.sharedMesh.name} bones {body.bones.Length} " +
               $"materials {string.Join(",", body.sharedMaterials.Select(m => m != null ? m.name + "/" + m.shader.name : "null"))}";
    }

    // Stops time where it is and takes the picture at the end of the frame.
    private IEnumerator Shoot(string pose)
    {
        float before = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        yield return new WaitForEndOfFrame();
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        string path = $"review/shots/current/outfit_{scene.ToLowerInvariant()}_{pose}.png";
        System.IO.File.WriteAllBytes(path, shot.EncodeToPNG());
        Say($"shot {path} {shot.width}x{shot.height}");
        Destroy(shot);
        Time.timeScale = before;
    }

    private IEnumerator Poses(string suffix, bool all)
    {
        Keys();
        yield return new WaitForSeconds(0.8f);
        yield return Shoot("idle" + suffix);

        // South, toward the camera, so the front of the outfit shows in every pose.
        Keys(Key.S);
        yield return new WaitForSeconds(0.55f);
        yield return Shoot("run" + suffix);
        Keys();
        yield return new WaitForSeconds(0.5f);
        if (!all) yield break;

        Keys(Key.Space);
        yield return null;
        Keys();
        yield return Until(() => player.Dodge.IsRolling, 0.5f);
        // Three moments of the roll: the dive, the middle of the tuck (where the body once sank through the floor) and the rise.
        var rolled = player.GetComponentInChildren<SkinnedMeshRenderer>(true);
        var hips = rolled.bones.FirstOrDefault(bone => bone.name == "hips");
        var head = rolled.bones.FirstOrDefault(bone => bone.name == "head");
        float floor = player.transform.position.y;
        yield return new WaitForSeconds(0.04f);
        Say($"roll{suffix}: hips {hips.position.y - floor:0.00} m head {head.position.y - floor:0.00} m above the feet");
        yield return Shoot("roll" + suffix);
        yield return new WaitForSeconds(0.13f);
        Say($"roll_mid{suffix}: hips {hips.position.y - floor:0.00} m head {head.position.y - floor:0.00} m bounds bottom {rolled.bounds.min.y - floor:0.00} m");
        yield return Shoot("roll_mid" + suffix);
        yield return new WaitForSeconds(0.08f);
        Say($"roll_out{suffix}: hips {hips.position.y - floor:0.00} m head {head.position.y - floor:0.00} m");
        yield return Shoot("roll_out" + suffix);
        yield return Until(() => !player.Dodge.IsRolling, 1.5f);
        yield return new WaitForSeconds(0.3f);

        Keys(Key.J);
        yield return null;
        Keys();
        yield return Until(() => combat.IsAttacking, 0.5f);
        yield return new WaitForSeconds(0.12f);
        yield return Shoot("attack" + suffix);
        yield return new WaitForSeconds(0.2f);
        Keys(Key.J);
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.22f);
        yield return Shoot("attack2" + suffix);
        yield return Until(() => !combat.IsAttacking, 2f);
    }

    // Beside the nearest enemy (or training dummy), with the game camera brought in close along its own angle.
    private IEnumerator Close(string suffix)
    {
        Transform foe = null;
        var enemies = FindObjectsByType<Enemy>().Where(enemy => !enemy.IsDead).Select(enemy => enemy.transform);
        var dummies = FindObjectsByType<TrainingDummy>().Select(dummy => dummy.transform);
        var all = enemies.Concat(dummies).ToArray();
        if (all.Length > 0) foe = all.OrderBy(t => (t.position - player.transform.position).sqrMagnitude).First();
        if (foe == null)
        {
            Say("close: no enemy or dummy in the scene");
            yield break;
        }
        // Side by side with it, turned three-quarters to the camera (which looks north), so the face and the front read.
        player.Teleport(foe.position + new Vector3(1.6f, 0f, -0.5f), Quaternion.LookRotation(new Vector3(-0.5f, 0f, -1f), Vector3.up));
        float distance = follow.Distance;
        follow.Distance = 5.5f;
        follow.SnapToTarget();
        yield return new WaitForSeconds(0.6f);
        yield return Shoot("close" + suffix);
        follow.Distance = distance;
        follow.SnapToTarget();
        Say($"close: beside {foe.name}");
    }

    private IEnumerator Start()
    {
        Find();
        bool chosenReal = PlayerPrefs.HasKey(Outfits.ChosenPrefsKey), lastReal = PlayerPrefs.HasKey(Outfits.LastPrefsKey);
        string lastRealValue = PlayerPrefs.GetString(Outfits.LastPrefsKey, "");
        Say($"keys: sandboxed {Clover.Core.Progress.Sandboxed}, writing to {Outfits.LastKey}; real chosen set {chosenReal}, real last '{lastRealValue}'");
        Say(Describe());
        yield return Hear();

        if (scene == "CombatTest")
        {
            foreach (string id in Outfits.All)
            {
                bool worn = outfit.Wear(id);
                yield return null;
                Say($"forced '{id}' worn {worn}: " + Describe());
                yield return Poses("_" + id, true);
                yield return Close("_" + id);
            }
        }
        else
        {
            var spawn = player.transform.position;
            yield return Poses("", scene != "Castle");
            // The same again with the game camera brought in, to look for stretching and popping.
            player.Teleport(spawn, Quaternion.identity);
            float distance = follow.Distance;
            follow.Distance = 5.5f;
            follow.SnapToTarget();
            yield return Poses("_near", scene != "Castle");
            follow.Distance = distance;
            follow.SnapToTarget();
            yield return Close("");
        }

        if (scene == "SunCity")
        {
            SceneManager.LoadScene("Hub");
            yield return null;
            yield return null;
            yield return new WaitForSeconds(1f);
            Find();
            Say("after Sun City: " + Describe());
            yield return Hear();
            yield return Shoot("from_suncity");
        }

        Say($"real keys untouched: chosen {PlayerPrefs.HasKey(Outfits.ChosenPrefsKey) == chosenReal}, last {PlayerPrefs.HasKey(Outfits.LastPrefsKey) == lastReal && PlayerPrefs.GetString(Outfits.LastPrefsKey, "") == lastRealValue}");
        Say("done");
        Destroy(gameObject);
    }
}
