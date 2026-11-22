using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

// Play Mode probe of Memories paid in play (way_build_plan.md 2.6), in Sun City (the Fire Yard the plan names was removed;
// Sun City is the Fire land's area with both a common kind in numbers and the Champion of the Gate), with saved progress
// sandboxed: the title guide closed with Escape and time shown to run; Heat given so its row shows; two of the area's
// enemies of one kind killed, which pays +1 once (the second pays nothing); the Champion killed, +4. For each payment one
// still of the amber light in flight and one of the "+N" line under the Shards, and the line's box checked clear of the
// Heat row's. Frames review/shots/current/memories_<n>.png.
//   bash AgentScripts/runprobe.sh SunCity MemoriesProbe 60
public static class MemoriesProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Memories Probe").AddComponent<MemoriesProbeRunner>();
        return "probe started";
    }
}

public class MemoriesProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerUpgrades upgrades;
    private PlayerDefence defence;
    private int ok, wrong;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
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
        yield return null;
    }

    private IEnumerator Still(string name)
    {
        float scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    // A box on the screen, from a UI element's corners.
    private static Rect ScreenBox(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var canvas = rect.GetComponentInParent<Canvas>();
        Camera eye = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 min = RectTransformUtility.WorldToScreenPoint(eye, corners[0]), max = min;
        for (int i = 1; i < 4; i++)
        {
            Vector2 at = RectTransformUtility.WorldToScreenPoint(eye, corners[i]);
            min = Vector2.Min(min, at);
            max = Vector2.Max(max, at);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // The line's box as its words fill it, not the whole width the line may take.
    private static Rect WordsBox(Text text)
    {
        var box = ScreenBox(text.rectTransform);
        float width = Mathf.Min(box.width, text.preferredWidth * box.width / Mathf.Max(1f, text.rectTransform.rect.width));
        float height = Mathf.Min(box.height, text.preferredHeight * box.height / Mathf.Max(1f, text.rectTransform.rect.height));
        float x = text.alignment == TextAnchor.UpperRight || text.alignment == TextAnchor.MiddleRight || text.alignment == TextAnchor.LowerRight ? box.xMax - width : box.xMin;
        return new Rect(x, box.yMax - height, width, height);
    }

    private IEnumerator Payment(Enemy[] killed, int expected, string label, int frame)
    {
        var wallet = Progress.Data.Wallet;
        var gain = MemoryGain.Current;
        int before = wallet.Memories;
        int landed = gain != null ? gain.Landed : 0;
        var body = killed[0];
        // On the camera's side of the body, so the light is seen leaving it and coming down to the player.
        Vector3 to = body.transform.position + Vector3.back * 4f;
        player.Teleport(new Vector3(to.x, body.transform.position.y + 0.1f, to.z), Quaternion.identity);
        yield return Real(0.5f);
        foreach (var enemy in killed) enemy.Kill();
        yield return null;
        int paid = wallet.Memories - before;
        Judge($"{label} pays +{expected}", paid == expected, $"{before} -> {wallet.Memories} Memories");
        yield return Real(0.35f);
        var light = FindObjectsByType<Light>(FindObjectsInactive.Exclude).FirstOrDefault(one => one.name.StartsWith("Memory Light"));
        Judge($"{label}: an amber light flies to the player", light != null && light.color.b < light.color.r * 0.5f,
            light != null ? $"colour {light.color}, at {light.transform.position}, player {player.transform.position}" : "no light");
        yield return Still($"memories_{frame}_light");
        yield return Until(() => gain != null && gain.Landed > landed, 2f);
        yield return Real(0.15f);
        var text = gain != null ? gain.GetComponent<Text>() : null;
        string want = MemoryLine.Words(expected);
        Judge($"{label}: the line says {want} under the Shards", text != null && text.enabled && text.text == want, text != null ? $"'{text.text}', shown {text.enabled}" : "no line");
        if (text != null && text.cachedTextGenerator.characterCountVisible <= 0)
            Say($"  line not drawn: active {text.gameObject.activeInHierarchy}, enabled {text.enabled}, colour {text.color}, renderer alpha {text.canvasRenderer.GetAlpha()}, culled {text.canvasRenderer.cull}, font {(text.font != null ? text.font.name : "none")} {text.fontSize}, chars {text.cachedTextGenerator.characterCountVisible}, rect {text.rectTransform.rect} at {text.rectTransform.anchoredPosition}, parent {text.transform.parent.name}, sibling {text.transform.GetSiblingIndex()} of {text.transform.parent.childCount}, materials {text.materialForRendering?.name}");
        yield return Still($"memories_{frame}_line");
        if (text != null)
        {
            var heat = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(rect => rect.name == "Heat" && rect.Find("Heat 1 Back") != null);
            var lineBox = WordsBox(text);
            bool clear = true;
            string boxes = "";
            if (heat != null)
                for (int i = 0; i < heat.childCount; i++)
                {
                    var pip = heat.GetChild(i) as RectTransform;
                    if (pip == null) continue;
                    var pipBox = ScreenBox(pip);
                    if (pipBox.Overlaps(lineBox)) clear = false;
                    if (i == 0 || i == heat.childCount - 1) boxes += $" pip {pipBox}";
                }
            var shards = text.transform.parent.GetComponentsInChildren<Text>().FirstOrDefault(other => other != text && other.text.StartsWith("Shards"));
            bool belowShards = shards == null || ScreenBox(shards.rectTransform).yMin >= lineBox.yMax - 1f;
            Judge($"{label}: the line sits clear of the Heat row (shown {heat != null && heat.gameObject.activeInHierarchy}) and under the Shards", heat != null && heat.gameObject.activeInHierarchy && clear && belowShards,
                $"line {lineBox};{boxes}; shards {(shards != null ? ScreenBox(shards.rectTransform).ToString() : "none")}");
        }
        yield return Real(2.2f);
    }

    private IEnumerator Start()
    {
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        defence = player.GetComponent<PlayerDefence>();
        Judge("saved progress is sandboxed", Progress.Sandboxed);
        if (!Progress.Sandboxed) { Finish(); yield break; }

        yield return Until(() => GuidePage.Showing, 5f);
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        yield return Real(0.2f);
        float gameTimeBefore = Time.time;
        yield return Real(0.6f);
        Judge("the guide is closed and time runs", !GuidePage.Showing && Time.timeScale > 0f && Time.time > gameTimeBefore,
            $"scale {Time.timeScale:0.##}, game time {gameTimeBefore:0.00} -> {Time.time:0.00}");

        // Heat's row shows once Heat is learned and Fire carried; both given here so the line can be judged beside it.
        player.GetComponent<PlayerElement>().Take(ElementKit.Fire());
        yield return Real(0.3f);
        upgrades.Give(Upgrade.ById(Upgrade.Heat));
        upgrades.RefreshNow();
        yield return Real(0.2f);
        Judge("Heat is learned, so its row shows", upgrades.ShowsHeat);

        var director = FindAnyObjectByType<AreaDirector>();
        if (director == null) { Judge("the area's director", false); Finish(); yield break; }
        var kinds = director.Enemies.Where(enemy => enemy != null && !enemy.IsDead && !enemy.Kind.IsElite && !enemy.Kind.IsBoss && !enemy.Kind.IsGod)
            .GroupBy(enemy => enemy.Kind.Name).OrderByDescending(group => group.Count()).FirstOrDefault();
        var champion = director.Enemies.FirstOrDefault(enemy => enemy != null && enemy.Kind.Name == EnemyKind.ChampionName);
        Judge("the area has two of one common kind and the Champion", kinds != null && kinds.Count() >= 2 && champion != null,
            $"{kinds?.Key} x{kinds?.Count()}, champion {champion != null}");
        if (kinds == null || kinds.Count() < 2 || champion == null) { Finish(); yield break; }
        Judge("neither is paid yet in this sandbox", !Progress.Data.KindsDefeated.Contains(Income.KeyOf(kinds.First().Kind)) && !Progress.Data.KindsDefeated.Contains(Income.KeyOf(champion.Kind)));

        yield return Payment(kinds.Take(2).ToArray(), 1, $"two of {kinds.Key}", 1);
        yield return Payment(new[] { champion }, 4, "the Champion", 2);
        Finish();
    }

    private void Finish()
    {
        Time.timeScale = 1f;
        Say($"summary: {ok} OK, {wrong} WRONG");
        Say("done");
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
