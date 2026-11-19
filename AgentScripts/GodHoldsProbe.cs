using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

// Play Mode probe for how ice and holds take on a god (docs/GAME.md, 2026-11-19): the man is frozen for a moment and
// then guarded, the godly form is frozen in the middle of what it is doing and its own clock stands still for a hitch
// (nothing skipped, nothing cancelled), then guarded, then held. Works in either god's scene. Frames:
// review/shots/current/godholds_<what>.png. Run with
//   bash AgentScripts/runprobe.sh Summit GodHoldsProbe 80
//   bash AgentScripts/runprobe.sh SunCourt GodHoldsProbe 80
public static class GodHoldsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("God Holds Probe").AddComponent<GodHoldsProbeRunner>();
        return "probe started";
    }
}

public class GodHoldsProbeRunner : MonoBehaviour
{
    private PlayerDefence defence;

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private static IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.time + timeout;
        while (!condition() && Time.time < end) yield return null;
    }

    private static IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot($"review/shots/current/godholds_{name}.png");
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        defence = player != null ? player.GetComponent<PlayerDefence>() : null;
        Enemy man = null;
        foreach (var enemy in FindObjectsByType<Enemy>())
            if (enemy.Kind != null && enemy.Kind.IsGod) man = enemy;
        var form = FindAnyObjectByType<GodlyForm>();
        if (man == null || form == null)
        {
            Say($"no god here: man {man != null} form {form != null}");
            Say("done");
            yield break;
        }
        int full = Afflictions.Depth(AfflictionKind.Chill);
        yield return new WaitForSeconds(1f);

        // 1: the man, frozen and then guarded.
        man.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.8f);
        man.Afflictions.Apply(AfflictionKind.Chill, full);
        float frozenAt = Time.time;
        Say($"1 man {man.name}: frozen {man.Afflictions.IsFrozen} for {man.Afflictions.FrozenLeft:0.00} s (rule {GodHolds.Seconds(2.5f * Afflictions.FreezeSeconds, GodPhase.Man):0.00}; an elite's {2.5f * 0.4f * Afflictions.FreezeSeconds:0.00}) rank {man.Afflictions.Rank}");
        yield return Shot("man_frozen");
        yield return Until(() => !man.Afflictions.IsFrozen, 3f);
        Say($"1 man thawed after {Time.time - frozenAt:0.00} s; guard {man.Afflictions.GuardLeft:0.00} s; a hold now takes {man.Afflictions.Hold(2f)}; chill laid now freezes {Refreeze(man.Afflictions, full)}");
        yield return Until(() => man.Afflictions.GuardLeft <= 0f, 4f);
        bool held = man.Afflictions.Hold(2f);
        Say($"1 man after the guard: a hold of 2 s takes {held} for {man.Afflictions.HeldLeft:0.00} s");

        // 2: the godly form, stopped in the middle of what it is doing.
        for (int blow = 0; blow < 200 && !man.IsDead; blow++) man.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => form.InFight && form.Doing != "Rising" && form.Doing != "Entering", 25f);
        yield return new WaitForSeconds(1.5f);
        yield return Until(() => form.Doing != "Idle" && form.DoingFor > 0.3f, 15f);
        string doing = form.Doing;
        float before = form.DoingFor;
        form.Afflictions.Apply(AfflictionKind.Chill, full);
        float frozenLeft = form.Afflictions.FrozenLeft;
        bool took = form.Afflictions.IsFrozen;
        yield return null;
        float stoppedAt = Time.time, least = form.DoingFor, most = form.DoingFor;
        int frames = 0;
        bool shot = false;
        while (form.IsStopped && Time.time - stoppedAt < 2f)
        {
            frames++;
            least = Mathf.Min(least, form.DoingFor);
            most = Mathf.Max(most, form.DoingFor);
            if (!shot)
            {
                shot = true;
                yield return Shot("form_frozen");
                continue;
            }
            yield return null;
        }
        float stoppedFor = Time.time - stoppedAt;
        string after = form.Doing;
        float resumedAt = form.DoingFor;
        yield return new WaitForSeconds(0.3f);
        Say($"2 form {form.name} frozen during {doing} at {before:0.00} s into it: took {took} for {frozenLeft:0.00} s (rule {GodHolds.Seconds(2.5f * Afflictions.FreezeSeconds, GodPhase.Godly):0.00}); "
            + $"stopped {stoppedFor:0.00} s over {frames} frames; its clock moved {most - least:0.000} s while stopped; after it {after} at {resumedAt:0.00} s, "
            + $"0.3 s later {form.Doing} at {form.DoingFor:0.00} s ({(after == doing ? "SAME ATTACK, CARRIED ON" : "MOVED ON")}); rank {form.Afflictions.Rank}");
        Say($"2 form guard {form.Afflictions.GuardLeft:0.00} s; a hold now takes {form.Afflictions.Hold(2f)}; chill laid now freezes {Refreeze(form.Afflictions, full)}");
        yield return Until(() => form.Afflictions.GuardLeft <= 0f, 6f);
        yield return Until(() => form.InFight && !form.IsStaggered, 6f);
        held = form.Afflictions.Hold(2f);
        float heldLeft = form.Afflictions.HeldLeft;
        yield return null;
        stoppedAt = Time.time;
        while (form.IsStopped && Time.time - stoppedAt < 2f) yield return null;
        Say($"3 form after the guard: a hold of 2 s takes {held} for {heldLeft:0.00} s (rule {GodHolds.Seconds(2f, GodPhase.Godly):0.00}); stopped {Time.time - stoppedAt:0.00} s; doing {form.Doing}");
        Say("done");
    }

    // Lays full Chill and says whether it froze: under the guard it must not.
    private static bool Refreeze(AfflictionSet set, int full)
    {
        set.Apply(AfflictionKind.Chill, full);
        return set.IsFrozen;
    }
}
