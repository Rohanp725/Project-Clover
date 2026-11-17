using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

// Play Mode probe for what the fight's camera can see of the storm dragon from every place a player can stand. Run with
//   bash AgentScripts/runprobe.sh Summit DragonViewProbe 200
// then  python AgentScripts/dragon_sheets.py </dev/null  to lay the frames out as sheets under review/shots/current/sheets.
// Parts (names with commas between in AgentScripts/dragon_view.txt; empty or missing runs all; add moves' names, such as TailScythe, to keep Ends and Things to those moves):
//   Rim    the body in the cloud from the middle, the four corners and the four sides, four frames 1.5 s apart each
//          (dragon_view_rim_<place>_<n>.png)
//   Ends   each move of the tail from each side it may come from, stopped when it is furthest out, seen from the eight
//          places (dragon_view_<move>_<side>_<place>.png): no cut end of any piece may be in any of them
//   Things each move of the tail from each side, aimed at a player near a post, pictured three times as it passes the
//          posts, the rail and the head (dragon_view_things_<move>_<side>_<n>.png)
public static class DragonViewProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Dragon View Probe").AddComponent<DragonViewRunner>();
        return "probe started";
    }
}

public class DragonViewRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private StormDragon dragon;
    private Vector3? hold;

    private static readonly Vector3[] Places =
    {
        new Vector3(-10.5f, 0.1f, -10.5f), new Vector3(10.5f, 0.1f, -10.5f), new Vector3(-10.5f, 0.1f, 10.5f), new Vector3(10.5f, 0.1f, 10.5f),
        new Vector3(0f, 0.1f, -15f), new Vector3(15f, 0.1f, 0f), new Vector3(0f, 0.1f, 13f), new Vector3(-15f, 0.1f, 0f),
    };
    private static readonly string[] PlaceNames = { "sw", "se", "nw", "ne", "s", "e", "n", "w" };

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
        if (hold.HasValue && player != null)
        {
            Vector3 off = player.transform.position - hold.Value;
            off.y = 0f;
            if (off.magnitude > 0.5f) player.Teleport(hold.Value, Quaternion.identity);
        }
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot($"review/shots/current/dragon_view_{name}.png");
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        dragon = FindAnyObjectByType<StormDragon>();
        var boss = FindAnyObjectByType<Enemy>();
        var view = FindAnyObjectByType<TopDownCamera>();
        const string onlyFile = "AgentScripts/dragon_view.txt";
        string only = System.IO.File.Exists(onlyFile) ? System.IO.File.ReadAllText(onlyFile).Trim() : "";
        string wanted = "," + only.Replace(" ", "") + ",";
        bool Wants(string part) => only == "" || wanted.Contains("," + part + ",");
        if (dragon == null || view == null)
        {
            Say("no storm dragon or no camera in this scene");
            Say("done");
            yield break;
        }
        var middle = new Vector3(0f, 0.1f, 2f);
        player.Teleport(middle, Quaternion.identity);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => dragon.HasRisen, 8f);
        dragon.RestBetween = 600f;
        yield return Until(() => dragon.Doing == "Idle", 10f);

        if (Wants("Rim"))
        {
            int least = 99, most = 0;
            for (int place = -1; place < Places.Length; place++)
            {
                Vector3 at = place < 0 ? middle : Places[place];
                player.Teleport(at, Quaternion.identity);
                hold = at;
                yield return new WaitForSeconds(1.6f);
                for (int i = 1; i <= 4; i++)
                {
                    least = Mathf.Min(least, dragon.LoopsRisen);
                    most = Mathf.Max(most, dragon.LoopsRisen);
                    yield return Shot($"rim_{(place < 0 ? "middle" : PlaceNames[place])}_{i}");
                    yield return new WaitForSeconds(1.5f);
                }
            }
            Say($"rim: loops standing out of the cloud at once, least {least} most {most}");
        }

        DragonAttack[] moves = { DragonAttack.TailSweep, DragonAttack.TailSlam, DragonAttack.TailCoil, DragonAttack.TailScythe, DragonAttack.TailFlops, DragonAttack.TailSting };
        // A move's name in the file (TailSweep, TailSlam, TailCoil, TailScythe, TailFlops, TailSting) keeps Ends and Things to the moves named.
        bool WantsMove(string move) => !wanted.Contains(",Tail") || wanted.Contains("," + move + ",");
        string[] moveNames = { "slither", "flop", "coil", "scythe", "flops", "sting" };
        float[] furthest =
        {
            dragon.TailSweepTelegraph + dragon.TailSweepSeconds * 0.95f, dragon.TailSlamTelegraph + dragon.TailSlamDrop + 0.14f, dragon.CoilTelegraph + dragon.CoilSqueeze * 0.9f,
            dragon.ScytheTelegraph + dragon.ScytheFirst * 0.5f, dragon.FlopsFirst + dragon.TailSlamDrop + dragon.FlopsNext + dragon.TailSlamDrop + 0.12f, dragon.StingRise + dragon.StingStab + 0.04f,
        };
        float[] sides = { 103f, -103f, 129f, -129f, 154f, -154f };
        string[] sideNames = { "east", "west", "southeast", "southwest", "southsoutheast", "southsouthwest" };

        // With AgentScripts/dragon_view_south.txt present every side is tried, whatever the fight itself allows, so that what each shows can be judged.
        if (System.IO.File.Exists("AgentScripts/dragon_view_south.txt")) dragon.TailNeverSouthOf = 180f;
        if (Wants("Ends"))
        {
            float follow = view.FollowTime, framing = view.FramingTime;
            for (int move = 0; move < moves.Length; move++)
                for (int side = 0; side < sides.Length; side++)
                {
                    string name = moves[move].ToString();
                    if (!WantsMove(name)) continue;
                    yield return Until(() => dragon.Doing == "Idle", 16f);
                    dragon.TailFromDegrees = sides[side];
                    dragon.Queued = moves[move];
                    player.Teleport(middle, Quaternion.identity);
                    hold = middle;
                    yield return Until(() => dragon.Doing == name, 8f);
                    yield return null;
                    float came = Mathf.Atan2(dragon.TailEntry.x, dragon.TailEntry.z) * Mathf.Rad2Deg;
                    float at = furthest[move];
                    yield return Until(() => dragon.DoingFor >= at || dragon.Doing != name, 10f);
                    if (dragon.Doing != name)
                    {
                        Say($"ends: {name} from {sideNames[side]} was over before it could be pictured");
                        continue;
                    }
                    // Time all but stopped, and a camera that is where it belongs at once, so one moment is seen from every place.
                    Time.timeScale = 0.01f;
                    view.FollowTime = view.FramingTime = 0.00002f;
                    for (int place = 0; place < Places.Length; place++)
                    {
                        player.Teleport(Places[place], Quaternion.identity);
                        hold = Places[place];
                        for (int frame = 0; frame < 8; frame++) yield return null;
                        yield return Shot($"{moveNames[move]}_{sideNames[side]}_{PlaceNames[place]}");
                    }
                    view.FollowTime = follow;
                    view.FramingTime = framing;
                    Time.timeScale = 1f;
                    hold = middle;
                    player.Teleport(middle, Quaternion.identity);
                    Say($"ends: {name} asked from {sides[side]:0} came over the rim at {came:0} degrees from north");
                    yield return Until(() => dragon.Doing != name, 20f);
                }
            dragon.TailFromDegrees = 999f;
        }

        if (Wants("Things"))
        {
            for (int move = 0; move < moves.Length; move++)
                for (int side = 0; side < sides.Length; side++)
                {
                    string name = moves[move].ToString();
                    if (!WantsMove(name)) continue;
                    yield return Until(() => dragon.Doing == "Idle", 16f);
                    dragon.TailFromDegrees = sides[side];
                    dragon.Queued = moves[move];
                    // Near the rim on the side it comes from, and a little round from it, so its way in passes close by the posts; for the east and west, north of that, so it passes the head.
                    Vector3 from = Quaternion.Euler(0f, sides[side] + 24f, 0f) * Vector3.forward * 11f;
                    var where = new Vector3(from.x, 0.1f, from.z);
                    player.Teleport(where, Quaternion.identity);
                    hold = where;
                    yield return Until(() => dragon.Doing == name, 8f);
                    float at = furthest[move];
                    // The scythe's third picture is of its second pass, coming back.
                    float[] moments = { at * 0.55f, at, moves[move] == DragonAttack.TailScythe ? dragon.ScytheTelegraph + dragon.ScytheFirst + dragon.ScythePause + dragon.ScytheSecond * 0.5f : at + 0.5f };
                    for (int i = 0; i < moments.Length; i++)
                    {
                        float moment = moments[i];
                        yield return Until(() => dragon.DoingFor >= moment || dragon.Doing != name, 10f);
                        yield return Shot($"things_{moveNames[move]}_{sideNames[side]}_{i + 1}");
                    }
                    Say($"things: {name} from {sideNames[side]}: least room between the tail and a post, the head or a forefoot {dragon.TailLeastRoom:0.00} m");
                    yield return Until(() => dragon.Doing != name, 20f);
                }
            dragon.TailFromDegrees = 999f;
        }
        hold = null;
        Say("done");
    }
}
