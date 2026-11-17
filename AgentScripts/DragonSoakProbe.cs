using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

// Holds the storm dragon in its costliest state for the profiler: the godly form is started directly and made to do
// its tail's moves (scythe, coil, slither, sting, flops) and then the beam, round and round for forty seconds with the
// body winding round it, while the player is kept alive in the middle.
// It writes "[Probe] beaming" when the first of them begins (point `diagnostics profiler_record` at it then), the
// garbage the managed heap gathered per frame over the soak, and "[Probe] done". Run it in the Summit scene.
public static class DragonSoakProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Dragon Soak Probe").AddComponent<DragonSoakRunner>();
        return "probe started";
    }
}

public class DragonSoakRunner : MonoBehaviour
{
    // The tail's moves first, since they lay a spine every frame; then the beam.
    private static readonly DragonAttack[] Cycle = { DragonAttack.TailScythe, DragonAttack.TailCoil, DragonAttack.TailSweep, DragonAttack.TailSting, DragonAttack.TailFlops, DragonAttack.Beam };
    private int next;
    private PlayerDefence defence;

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var boss = FindAnyObjectByType<Enemy>();
        var dragon = FindAnyObjectByType<StormDragon>();
        player.Teleport(new Vector3(0f, 0.1f, 2f), Quaternion.identity);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        while (!dragon.InFight) yield return null;
        dragon.RestBetween = 60f;
        bool said = false;
        float end = Time.time + 40f;
        int frames = 0;
        long before = 0;
        while (Time.time < end)
        {
            if (dragon.Doing == "Idle" && dragon.Queued == DragonAttack.None) dragon.Queued = Cycle[next++ % Cycle.Length];
            if (!said && dragon.Doing != "Idle")
            {
                said = true;
                Debug.Log("[Probe] beaming");
                before = System.GC.GetTotalMemory(false);
            }
            if (said) frames++;
            yield return null;
        }
        long grown = System.GC.GetTotalMemory(false) - before;
        Debug.Log($"[Probe] soak: {frames} frames, managed heap grew {grown} bytes ({(frames > 0 ? grown / frames : 0)} a frame, everything in the scene counted); beams {dragon.Beams}; loops standing {dragon.LoopsRisen}");
        Debug.Log("[Probe] done");
    }
}
