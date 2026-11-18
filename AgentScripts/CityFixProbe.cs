using System;
using System.Collections;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the fixes to the owner's Sun City play of 2026-11-18 (bash AgentScripts/runprobe.sh SunCity CityFixProbe 90):
//  6. a thirsting on the riverbed forced through every blow it strikes by hand at a player on the barque's deck over it
//     (none may land), then at a player beside it on the bed (they must); the player's light chain from the deck at it
//     (no damage) and beside it (damage).
//  7. walking into the back of each boat-shed (where the plain's sand showed and the player fell for ever).
//  8. walking off the old open edges: the banks' south ends (and the whole walk south down the fishers' bank from the
//     top of its ramp), the quay's west end, the lane level's east gap, the roofs' east edge.
//  the safety net: the player put far under the land must come back up on firm ground.
//  5. walking into the barque's shrine and its prow from the deck; frames of the fixes (review/shots/current/fix_<n>_<what>.png).
// Lines that can be judged end OK or WRONG.
public static class CityFixProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("City Fix Probe").AddComponent<CityFixProbeRunner>();
        return "probe started";
    }
}

public class CityFixProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private AreaDirector director;
    private bool keepAlive = true;
    private int oks, wrongs;

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private string Judge(bool good)
    {
        if (good) oks++; else wrongs++;
        return good ? "OK" : "WRONG";
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
        yield return null;
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/fix_" + name + ".png");
        yield return null;
        yield return null;
    }

    private void Calm()
    {
        foreach (var enemy in director.Enemies)
            if (enemy != null && !enemy.IsDead) enemy.Restore();
    }

    private void Go(Vector3 at, Vector3? face = null) =>
        player.Teleport(at + Vector3.up * 0.1f, face.HasValue && face.Value.sqrMagnitude > 0.01f ? Quaternion.LookRotation(face.Value) : Quaternion.identity);

    private static void Put(Enemy enemy, Vector3 at, Vector3 toward)
    {
        var body = enemy.GetComponent<CharacterController>();
        if (body != null) body.enabled = false;
        Vector3 look = toward - at;
        look.y = 0f;
        enemy.transform.SetPositionAndRotation(at + Vector3.up * 0.05f, Quaternion.LookRotation(look.sqrMagnitude > 0.01f ? look : Vector3.forward));
        if (body != null) body.enabled = true;
    }

    // Holds a key from a place; says where the player ended and the lowest they went on the way.
    private IEnumerator Walk(string what, Vector3 from, Key key, float seconds, Func<Vector3, float, bool> good)
    {
        Calm();
        Go(from);
        yield return new WaitForSeconds(0.3f);
        Keys(key);
        float end = Time.time + seconds, lowest = float.MaxValue;
        while (Time.time < end)
        {
            Calm();
            lowest = Mathf.Min(lowest, player.transform.position.y);
            yield return null;
        }
        Keys();
        yield return new WaitForSeconds(0.3f);
        lowest = Mathf.Min(lowest, player.transform.position.y);
        Vector3 at = player.transform.position;
        Say($"   {what}: from {from:0.0} ended {at:0.0}, lowest {lowest:0.00} {Judge(good(at, lowest))}");
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        director = FindAnyObjectByType<AreaDirector>();
        var catcher = FindAnyObjectByType<FallCatch>();
        yield return new WaitForSeconds(1.5f);
        var box = ReadingBox.Instance;
        for (int i = 0; i < 30 && box != null && box.IsOpen; i++)
        {
            yield return Tap(Key.E);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        var barque = GameObject.Find("World/Dry River/barque").transform;

        // ------------------------------------------------------------ 6: blows by hand and height
        var thirst = director.Enemies.First(enemy => enemy != null && enemy.Kind.Name == EnemyKind.ThirstName && !enemy.IsDead);
        foreach (var other in director.Enemies)
            if (other != null && other != thirst && !other.IsDead && Vector3.Distance(other.transform.position, barque.position) < 30f) other.gameObject.SetActive(false);
        Vector3 onDeck = barque.TransformPoint(new Vector3(2.4f, 4.8f, 5f));
        Vector3 below = barque.TransformPoint(new Vector3(5.2f, 0f, 5f));
        below.y = 0f;
        Vector3 beside = below + (onDeck - below).normalized * 0f + Vector3.ProjectOnPlane(onDeck - below, Vector3.up).normalized * 1.6f;
        beside.y = 0f;
        var moves = thirst.Brain.Attacks.Where(attack => !attack.Ranged && !attack.Casts && !attack.Beams && attack.NeedsFlasks == 0).Select(attack => attack.Name).ToArray();
        Say($"6 the thirsting's blows by hand: {string.Join(", ", moves)}; horizontal gap deck to it {Vector3.ProjectOnPlane(onDeck - below, Vector3.up).magnitude:0.0} m, height {onDeck.y - below.y:0.0} m");
        foreach (var (where, at) in new[] { ("on the deck over it", onDeck), ("beside it on the bed", new Vector3(beside.x, 0f, beside.z)) })
        {
            int landed = 0, struck = 0;
            foreach (var move in moves)
            {
                thirst.Restore();
                yield return null;
                Put(thirst, below, at);
                Go(at, below - at);
                yield return new WaitForSeconds(0.2f);
                int taken = defence.BlowsTaken, blows = thirst.BlowsStruck;
                thirst.Brain.Alert();
                bool forced = thirst.Brain.Force(move);
                var attack = thirst.Brain.Current;
                if (where.StartsWith("on")) yield return Until(() => thirst.Brain.TelegraphProgress > 0.8f, 3f);
                if (where.StartsWith("on")) yield return Shot("6_deck_" + move);
                yield return Until(() => thirst.Brain.State == EnemyState.Recover || thirst.Brain.Current != attack, 4f);
                yield return new WaitForSeconds(0.5f);
                landed += defence.BlowsTaken - taken;
                struck += thirst.BlowsStruck - blows;
                Go(at, below - at);
            }
            bool fine = where.StartsWith("on") ? struck > 0 && landed == 0 : landed > 0;
            Say($"   forced on a player {where}: blows struck {struck}, landed {landed} {Judge(fine)}");
        }
        // The other way: the player's light chain at it, from the deck and from beside it.
        foreach (var (where, at) in new[] { ("from the deck", onDeck), ("from beside it", new Vector3(beside.x, 0f, beside.z)) })
        {
            thirst.Restore();
            yield return null;
            Put(thirst, below, at);
            Go(at, below - at);
            yield return new WaitForSeconds(0.3f);
            thirst.Brain.Occupy(3f);
            float before = thirst.Health.Current;
            for (int i = 0; i < 4; i++)
            {
                Go(at, below - at);
                yield return Tap(Key.J);
                yield return new WaitForSeconds(0.35f);
            }
            yield return new WaitForSeconds(0.4f);
            float lost = before - thirst.Health.Current;
            Say($"   the player's light chain {where}: it lost {lost:0} {Judge(where.Contains("deck") ? lost == 0f : lost > 0f)}");
        }
        thirst.Restore();
        foreach (var other in director.Enemies)
            if (other != null && !other.gameObject.activeSelf) other.gameObject.SetActive(true);
        Calm();

        // ------------------------------------------------------------ 7: the boat-sheds' backs
        Say("7 into the back of each boat-shed:");
        foreach (float x in new[] { -18f, -8f, 8f, 18f })
            yield return Walk($"shed at x {x}", new Vector3(x, 0f, 116.6f), Key.W, 1.3f, (at, lowest) => lowest > -0.4f && at.y > -0.4f);
        yield return Walk("shed at x -8, back to the quay stair", new Vector3(-8f, 0f, 118.2f), Key.D, 1.2f, (at, lowest) => lowest > -0.4f);
        Go(new Vector3(-8f, 0f, 117.5f));
        yield return new WaitForSeconds(1.2f);
        yield return Shot("7_boatshed");

        // ------------------------------------------------------------ 8: the open edges
        Say("8 off the old open edges:");
        yield return Walk("fishers' bank, from the top of its ramp all the way south", new Vector3(-23.5f, 3f, 97f), Key.S, 18f, (at, lowest) => lowest > 2.5f && at.z > -5.8f);
        yield return Walk("west bank's south end", new Vector3(-30f, 3f, -2f), Key.S, 1.6f, (at, lowest) => lowest > 2.5f && at.z > -5.8f);
        yield return Walk("east bank's south end", new Vector3(20f, 3f, -2f), Key.S, 1.6f, (at, lowest) => lowest > 2.5f && at.z > -5.8f);
        yield return Walk("quay's west end", new Vector3(-37f, 4f, 125f), Key.A, 1.6f, (at, lowest) => lowest > 3.5f && at.x > -40.5f);
        yield return Walk("lane level's east gap", new Vector3(35f, 4f, 135.5f), Key.D, 1.6f, (at, lowest) => lowest > 3.5f && at.x < 38.5f);
        yield return Walk("roofs' east edge", new Vector3(37f, 10f, 162f), Key.D, 1.6f, (at, lowest) => lowest > 9.5f && at.x < 40f);
        Go(new Vector3(-30f, 3f, -3f));
        yield return new WaitForSeconds(1.2f);
        yield return Shot("8_bank_end");
        Go(new Vector3(36f, 10f, 162f));
        yield return new WaitForSeconds(1.2f);
        yield return Shot("8_roof_edge");

        // ------------------------------------------------------------ the safety net
        Calm();
        Go(new Vector3(-10f, 3f, 40f));
        yield return new WaitForSeconds(1f);
        int catches = catcher != null ? catcher.Catches : -1;
        Vector3 firm = catcher != null ? catcher.LastFirm : Vector3.zero;
        player.Teleport(new Vector3(-60f, -12f, -30f), Quaternion.identity);
        yield return new WaitForSeconds(0.6f);
        Vector3 back = player.transform.position;
        Say($"net: put at (-60, -12, -30), now at {back:0.0}, last firm ground {firm:0.0}, catches +{(catcher != null ? catcher.Catches - catches : -1)} {Judge(catcher != null && catcher.Catches == catches + 1 && back.y > -0.5f && (back - firm).magnitude < 1.5f)}");
        player.Teleport(new Vector3(0f, -4f, 300f), Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Say($"net: dropped from -4 under the avenue, now at {player.transform.position:0.0} {Judge(player.transform.position.y > -0.5f && catcher != null && catcher.Catches == catches + 2)}");

        // ------------------------------------------------------------ 5: the barque's deck
        Say("5 the barque:");
        yield return Walk("gangplank up the west side", new Vector3(-6.3f, 0f, 83.3f), Key.D, 1.4f, (at, lowest) => Mathf.Abs(at.y - 3.5f) < 0.35f);
        // On the shrine's line: W walks the boat's own -z, drifting a third of that to its +x.
        Vector3 aft = barque.TransformPoint(new Vector3(-1.6f, 4.8f, 4.4f));
        yield return Walk("from the aft deck into the shrine", aft, Key.W, 1.2f, (at, lowest) => barque.InverseTransformPoint(at).z > 2.8f && Mathf.Abs(at.y - 3.5f) < 0.4f);
        Say($"     stopped at the boat's own z {barque.InverseTransformPoint(player.transform.position).z:0.0} (the shrine's aft face is at 2.8)");
        yield return Shot("5_shrine");
        Vector3 fore = barque.TransformPoint(new Vector3(-0.9f, 4.8f, -7f));
        Go(fore);
        yield return new WaitForSeconds(0.3f);
        // W drifts toward the boat's +x, so the body slides along the post's face and off the deck's side to the bed (a
        // drop the land allows); what matters is that it never stood inside the post's box.
        yield return Walk("from the fore deck into the prow", fore, Key.W, 1.4f, (at, lowest) =>
        {
            var local = barque.InverseTransformPoint(at);
            return !(local.z < -9.7f && local.x > -3.6f && local.x < 1.7f && at.y > 3f);
        });
        yield return Shot("5_prow");
        Vector3 stern = barque.TransformPoint(new Vector3(-0.5f, 4.8f, 9.6f));
        yield return Walk("from the aft deck into the stern", stern, Key.S, 1.2f, (at, lowest) => barque.InverseTransformPoint(at).z < 11.6f);

        // ------------------------------------------------------------ frames: 1, 2, 9
        Calm();
        foreach (var (name, at, wait) in new[]
        {
            ("1_bank_edge", new Vector3(22f, 3f, 104f), 1.2f), ("1_jam", new Vector3(-11f, 0f, 63.5f), 1.2f), ("1_fishers_bank", new Vector3(-30f, 3f, 36f), 1.2f),
            ("9_hut_behind", new Vector3(-33.75f, 3f, 30.5f), 1.6f), ("9_hut_beside", new Vector3(-29.5f, 3f, 26f), 1.6f),
            ("2_roof_room_behind", new Vector3(33f, 10f, 179.6f), 1.6f), ("9_lane_houses_behind", new Vector3(-20f, 6f, 162f), 1.6f),
        })
        {
            Go(at);
            yield return new WaitForSeconds(wait);
            yield return Shot(name);
            var faded = FindObjectsByType<Fadeable>(FindObjectsInactive.Exclude).Count(piece => piece.Fade < 0.99f);
            Say($"   frame {name}: {faded} pieces thinned out");
        }

        Say($"tally: {oks} OK, {wrongs} WRONG");
        Say("done");
    }
}
