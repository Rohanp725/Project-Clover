using System.Collections;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

// Play Mode probe for how afflictions are seen on a body. In the combat arena it stands three dummies in a row and
// lays each of the eleven afflictions on them, three at a time (two deep on the first, at full depth on the third),
// photographing each set to review/shots/current/affliction_<n>.png.
//   bash AgentScripts/runprobe.sh CombatTest AfflictionsProbe 60
public static class AfflictionsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Afflictions Probe").AddComponent<AfflictionsProbeRunner>();
        return "probe started";
    }
}

public class AfflictionsProbeRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None).Where(dummy => dummy.GetComponent<DummyAttacker>() == null).Take(3).ToList();
        var spots = new[] { new Vector3(4.2f, 0f, -2f), new Vector3(6.4f, 0f, -2f), new Vector3(8.6f, 0f, -2f) };
        for (int i = 0; i < dummies.Count; i++) dummies[i].transform.position = spots[i];
        Physics.SyncTransforms();
        player.Teleport(new Vector3(6.4f, 0f, -5.2f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        int shot = 0;
        for (int kind = 1; kind < Afflictions.Count; kind += 3)
        {
            string names = "";
            for (int i = 0; i < dummies.Count; i++)
            {
                dummies[i].Afflictions.Clear();
                if (kind + i >= Afflictions.Count) continue;
                var affliction = (AfflictionKind)(kind + i);
                dummies[i].Afflictions.Apply(affliction, kind + i == (int)AfflictionKind.Chill ? 5 : 3, 0f, 1f);
                names += affliction + "x" + dummies[i].Afflictions.Stacks(affliction) + " ";
            }
            yield return new WaitForSeconds(1.3f);
            ScreenCapture.CaptureScreenshot($"review/shots/current/affliction_{++shot}.png");
            yield return null;
            Debug.Log($"[Probe] shot {shot}: {names}pips {string.Join(",", dummies.Select(d => d.GetComponentInChildren<EnemyBars>() != null ? d.GetComponentInChildren<EnemyBars>().PipsShown : -1))}");
        }
        Debug.Log("[Probe] done");
    }
}
