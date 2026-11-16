using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;

/// <summary>
/// Castle: the grass holds still from frame to frame, the door opens once the Gate Captain alone has fallen and
/// stays open after a rest, enemies felled a second time pay nothing, and a Katana already won is not in the stone.
/// Run it twice: the second run (with the Katana remembered) checks the empty stone, then forgets it.
/// </summary>
public static class FixProbe
{
    public static void Run()
    {
        var runner = new GameObject("FixProbe").AddComponent<FixProbeRunner>();
        Debug.Log("[Probe] started");
    }
}

public class FixProbeRunner : MonoBehaviour
{
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var defence = player.GetComponent<PlayerDefence>();
        var interaction = player.GetComponent<PlayerInteraction>();
        var director = FindAnyObjectByType<AreaDirector>();
        var pickup = FindAnyObjectByType<WeaponPickup>();
        var way = FindAnyObjectByType<SealedWay>();
        bool remembered = Progress.Has(Progress.WeaponKey(Weapons.KatanaName));
        yield return new WaitForSeconds(0.5f);
        Say($"katana remembered {remembered}; stone holds it {pickup.Available}");

        // Grass: stand in the big patch of the outer court and compare two frames a moment apart.
        player.Teleport(new Vector3(-9f, 0.1f, 36f), Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 0f;
        yield return new WaitForEndOfFrame();
        var first = ScreenCapture.CaptureScreenshotAsTexture();
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        var second = ScreenCapture.CaptureScreenshotAsTexture();
        var a = first.GetPixels32();
        var b = second.GetPixels32();
        int differing = 0;
        for (int i = 0; i < a.Length; i++)
            if (Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b) > 24) differing++;
        System.IO.File.WriteAllBytes("review/shots/current/fix_grass.png", first.EncodeToPNG());
        Time.timeScale = 1f;
        Say($"grass: {differing} of {a.Length} pixels differ between two frames with time stopped");

        // The door, before the captain falls.
        way.Use(interaction);
        string before = defence.LastCallout;
        Enemy captain = null;
        foreach (var enemy in director.Enemies)
            if (enemy.Kind.Name == EnemyKind.CaptainName) captain = enemy;
        int embers = RunState.Points;
        captain.TakeHit(new Hit(100000f, 0f, Vector3.forward, true, false, captain.transform.position));
        yield return new WaitForSeconds(1f);
        int afterFirst = RunState.Points;
        // A rest brings him back; the seal stays broken, and he pays nothing the second time.
        director.Rest(player.transform);
        yield return new WaitForSeconds(0.5f);
        bool back = !captain.IsDead;
        captain.TakeHit(new Hit(100000f, 0f, Vector3.forward, true, false, captain.transform.position));
        yield return new WaitForSeconds(1f);
        Say($"door before: '{before}'; captain dead {captain.IsDead}; embers {embers} -> {afterFirst} -> after a rest and a second kill {RunState.Points} (he came back: {back}); seal broken {way.SealBroken}; enemies still alive {director.EnemiesAlive}");
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        if (!remembered)
        {
            pickup.Use(interaction);
            Say($"katana drawn: taken {pickup.Taken}, remembered {Progress.Has(Progress.WeaponKey(Weapons.KatanaName))}");
        }
        Say("done");
    }
}
