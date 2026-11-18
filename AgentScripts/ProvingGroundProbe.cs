using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe of the combat playtest's Proving Ground, with the real keys wherever a player would press one: the
// title and what is new; an element taken at an altar; weapons from the rack, a god's among them, and a stand of what
// is not built yet; a pen's enemies let out alone and together, and cleared; the dummies struck and the afflicted row;
// a rest (unlimited Shards), marks bought, a god's gift bought, the chart walked into another tree, a mark's details
// and the guide from the chart and from the pause menu; a fall with nothing lost; the door into Sun City and the pause
// menu's way back. Frames go to review/shots/current/pg_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround ProvingGroundProbe 120
public static class ProvingGroundProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Proving Ground Probe").AddComponent<ProvingGroundProbeRunner>();
        return "probe started";
    }
}

public class ProvingGroundProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerInteraction interaction;
    private bool keepAlive = true;
    private int ok, wrong;

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
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

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        interaction = player.GetComponent<PlayerInteraction>();
    }

    // Stands the player just south of the thing, facing it, and presses use once it is the thing chosen.
    private IEnumerator Use(Interactable thing, float back = 0.8f)
    {
        Vector3 at = thing.transform.position + Vector3.back * back;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.35f);
        if (interaction.Current != thing) Say($"  (chosen is {(interaction.Current != null ? interaction.Current.name : "nothing")}, not {thing.name})");
        yield return Tap(Key.E);
        yield return Real(0.2f);
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
    }

    private static T Find<T>(Func<T, bool> which) where T : UnityEngine.Object =>
        FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(which);

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        FindPlayer();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}");
        Judge("playtest mode is on in the Proving Ground", PlaytestMode.On);

        // 1. The title: the version and what is new, read at the player's pace.
        yield return Until(() => GuidePage.Showing, 5f);
        var page = GuidePage.Instance;
        Judge("the title opens on arrival", page != null && page.IsOpen && page.IsTitle, page != null ? $"\"{page.ShownTitle}\", {page.PageCount} page(s), time {Time.timeScale:0}" : "no page");
        yield return Real(0.6f);
        yield return Shot("pg_title");
        int pages = page != null ? page.PageCount : 0;
        for (int i = 0; i < pages && GuidePage.Showing; i++) yield return Tap(Key.E);
        yield return Real(0.2f);
        Judge("the title is read through with the use button", !GuidePage.Showing && Time.timeScale > 0f && !player.Suspended, $"time {Time.timeScale:0}, suspended {player.Suspended}");
        yield return ReadAll();

        // 2. The whole ground from the plaza.
        player.Teleport(new Vector3(0f, 0.05f, -6.5f), Quaternion.identity);
        yield return Real(1.2f);
        yield return Shot("pg_play_plaza");

        // 3. An element at an altar.
        var ice = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Ice);
        yield return Use(ice, 1.0f);
        Judge("the Ice altar gives Ice", element.Kit != null && element.Kit.Name == Elements.Ice, element.Kit != null ? element.Kit.Name : "none");

        // 4. Weapons from the rack: a plain one, the gods' two, and a stand of what is not built yet.
        foreach (string name in new[] { Weapons.GreatHammerName, Weapons.StormKatanaName, Weapons.SunSpearName })
        {
            var stand = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == name);
            yield return Use(stand, 1.0f);
            Judge($"the rack gives the {name}", combat.Weapon != null && combat.Weapon.Name == name, combat.Weapon != null ? combat.Weapon.Name : "none");
        }
        player.Teleport(new Vector3(-27f, 0.05f, 0.5f), Quaternion.identity);
        yield return Real(0.8f);
        yield return Shot("pg_play_rack");
        var slot = Find<Notice>(notice => notice.name.StartsWith("PG Not Built"));
        yield return Use(slot, 1.0f);
        bool read = ReadingBox.Instance != null && ReadingBox.Instance.IsOpen;
        yield return Shot("pg_not_built");
        yield return ReadAll();
        Judge("a stand of what is not built yet says so", read && slot != null, slot != null ? slot.Lines[0] : "none");

        // 5. The pens: one enemy, a group, cleared; the Fire land's too.
        var castle = Find<EnemyPen>(pen => pen.Land == "The castle");
        var fire = Find<EnemyPen>(pen => pen.Land == "The Fire land");
        var soldierPost = Find<PenPost>(post => post.What == EnemyKind.SoldierName);
        yield return Use(soldierPost, 0.9f);
        Judge("a post lets out one soldier", castle.Out == 1 && castle.Enemies[0].gameObject.activeSelf, $"out {castle.Out}");
        var threePost = Find<PenPost>(post => post.What.StartsWith("The castle's three"));
        yield return Use(threePost, 0.9f);
        Judge("a post lets out the castle's three together", castle.Out == 3, $"out {castle.Out}");
        var cityPost = Find<PenPost>(post => post.What.StartsWith("The city's people"));
        yield return Use(cityPost, 0.9f);
        Judge("the Fire land's post lets out four of its people", fire.Out == 4, $"out {fire.Out}");
        player.Teleport(new Vector3(-13f, 0.05f, 21.2f), Quaternion.identity);
        yield return Real(1.0f);
        yield return Shot("pg_play_pen_castle");
        player.Teleport(new Vector3(13f, 0.05f, 21.2f), Quaternion.identity);
        yield return Real(1.0f);
        yield return Shot("pg_play_pen_fire");
        // Into the castle's pen, to be noticed and to land a few blows.
        var soldier = castle.Enemies[0];
        player.Teleport(soldier.transform.position + Vector3.back * 2f, Quaternion.identity);
        yield return Real(0.4f);
        float before = soldier.Health.Current;
        for (int i = 0; i < 4; i++) yield return Tap(Key.J);
        yield return Real(1.2f);
        Judge("a penned soldier fights and is struck", soldier.Health.Current < before || soldier.IsDead, $"health {before:0} to {soldier.Health.Current:0}, aware {soldier.IsAware}");
        var clearCastle = Find<PenPost>(post => post.name == "Pen Post Clear" && post.transform.position.x < 0f);
        yield return Use(clearCastle, 0.9f);
        var clearFire = Find<PenPost>(post => post.name == "Pen Post Clear" && post.transform.position.x > 0f);
        yield return Use(clearFire, 0.9f);
        Judge("the clear posts empty both pens", castle.Out == 0 && fire.Out == 0, $"castle {castle.Out}, fire {fire.Out}");
        var wickPost = Find<PenPost>(post => post.What == EnemyKind.WickName);
        yield return Use(wickPost, 0.9f);
        Judge("the wick alone", fire.Out == 1, $"out {fire.Out}");
        yield return Use(clearFire, 0.9f);

        // 6. The dummies.
        var still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        player.Teleport(still.transform.position + Vector3.back * 1.6f, Quaternion.identity);
        yield return Real(0.3f);
        int hits = still.HitsTaken;
        for (int i = 0; i < 3; i++) yield return Tap(Key.J);
        yield return Real(0.8f);
        Judge("the still dummy takes blows", still.HitsTaken > hits, $"hits {hits} to {still.HitsTaken}");
        var fighter = Find<DummyAttacker>(dummy => true);
        player.Teleport(fighter.transform.position + Vector3.back * 2.2f, Quaternion.identity);
        yield return Real(4.5f);
        Judge("the dummy that fights back strikes", fighter.BlowsStruck > 0, $"blows {fighter.BlowsStruck}");
        var afflicted = FindObjectsByType<AfflictedDummy>();
        int carrying = afflicted.Count(dummy => dummy.GetComponent<TrainingDummy>().Afflictions.Has(dummy.Kind));
        Judge("each afflicted dummy carries its affliction", afflicted.Length == Elements.Count && carrying == afflicted.Length, $"{carrying} of {afflicted.Length}");
        player.Teleport(new Vector3(27f, 0.05f, -8.4f), Quaternion.identity);
        yield return Real(1.0f);
        yield return Shot("pg_play_dummies");

        // 7. A rest: unlimited Shards, marks bought, the chart walked into another tree, a god's gift.
        var rest = Find<RestPoint>(point => true);
        yield return Use(rest, 1.4f);
        var chart = UpgradeMenu.Instance;
        Judge("the rest fire opens the chart with unlimited Shards", chart.IsOpen && upgrades.Points >= PlaytestMode.RestShards, $"Shards {upgrades.Points}, viewing {chart.Viewed}");
        var iceRoot = chart.Chosen;
        yield return Tap(Key.E);
        Judge("the held tree's root is bought", iceRoot != null && upgrades.Owns(iceRoot.Id), iceRoot != null ? iceRoot.Name : "none");
        yield return Tap(Key.J);
        string next = chart.Viewed;
        Judge("light attack walks the chart to the next tree", next != Elements.Ice && chart.Chosen != null && chart.Chosen.BelongsTo(next), $"now {next}, on {(chart.Chosen != null ? chart.Chosen.Name : "nothing")}");
        var otherRoot = chart.Chosen;
        yield return Tap(Key.E);
        Judge("another tree's root is bought though Ice is held", otherRoot != null && upgrades.Owns(otherRoot.Id), otherRoot != null ? otherRoot.Name : "none");
        var gift = Gifts.SignatureOf(next);
        chart.Choose(gift.Id);
        yield return Tap(Key.E);
        Judge("a god's gift is bought as a playtest unlock", upgrades.Owns(gift.Id), gift.Name);
        yield return Real(0.3f);
        yield return Shot("pg_chart_other_tree");
        // Every tree round the ring can be reached with the guard and light buttons.
        int trees = 0;
        string seen = "";
        for (int i = 0; i < Elements.Count; i++)
        {
            yield return Tap(Key.LeftShift);
            if (!seen.Contains(chart.Viewed + ",")) { seen += chart.Viewed + ","; trees++; }
        }
        Judge("the guard button walks back round every tree", trees == Elements.Count, seen);

        // 8. Details of a mark with written words, and of one read from its data; the guide from the chart.
        chart.View(Elements.Fire);
        chart.Choose(Upgrade.Backdraft);
        yield return Tap(Key.Q);
        Judge("the spell button opens a mark's details", GuidePage.Showing && !GuidePage.Instance.IsList && GuidePage.Instance.ShownWords.Contains("What it does"), $"\"{GuidePage.Instance.ShownTitle}\", {GuidePage.Instance.PageCount} page(s)");
        yield return Real(0.3f);
        yield return Shot("pg_details");
        int detailPages = GuidePage.Instance.PageCount;
        for (int i = 0; i < detailPages && GuidePage.Showing; i++) yield return Tap(Key.E);
        Judge("the details are read through and close, the chart still open", !GuidePage.Showing && chart.IsOpen);
        chart.View(Elements.Metal);
        var keystone = Upgrade.All.First(mark => mark.Element == Elements.Metal && !mark.IsMix && mark.Kind == UpgradeKind.Keystone);
        chart.Choose(keystone.Id);
        yield return Tap(Key.Q);
        Judge("a tree without written words shows details from its data", GuidePage.Showing && GuidePage.Instance.ShownWords.Contains("The numbers"), keystone.Name);
        yield return Real(0.3f);
        yield return Shot("pg_details_auto");
        yield return Tap(Key.Space);
        yield return Tap(Key.R);
        Judge("the flask button opens the guide to effects", GuidePage.Showing && GuidePage.Instance.IsList && GuidePage.Instance.EntryCount >= 20, $"{GuidePage.Instance.EntryCount} entries");
        for (int i = 0; i < 3; i++) yield return Tap(Key.S);
        yield return Real(0.3f);
        yield return Shot("pg_guide");
        Say($"  guide on \"{GuidePage.Instance.ShownWords.Split('\n')[0]}\"");
        yield return Tap(Key.Space);
        Judge("the guide closes back to the chart", !GuidePage.Showing && chart.IsOpen);
        yield return Tap(Key.Space);
        yield return Real(0.3f);
        Judge("the chart closes and the world runs", !chart.IsOpen && Time.timeScale > 0f && !player.Suspended);

        // 9. The guide from the pause menu.
        yield return Tap(Key.Escape);
        var menu = PauseMenu.Instance;
        int guideAt = -1;
        for (int i = 0; i < menu.Choices.Count; i++) if (menu.Choices.At(i) == PauseEntry.Guide) guideAt = i;
        for (int i = 0; i < guideAt; i++) yield return Tap(Key.S);
        yield return Tap(Key.E);
        Judge("the pause menu opens the guide", GuidePage.Showing && GuidePage.Instance.IsList, $"entries in the menu {menu.Choices.Count}");
        yield return Real(0.3f);
        yield return Shot("pg_guide_pause");
        yield return Tap(Key.Escape);
        Judge("Esc closes the guide back to the pause menu", !GuidePage.Showing && menu.IsOpen);
        yield return Tap(Key.Escape);
        yield return Real(0.2f);

        // 10. A fall loses nothing: the player stands again at the fire.
        int shards = upgrades.Points, learned = RunState.Unlocked.Count;
        string weapon = combat.Weapon.Name, held = element.Kit.Name;
        player.Teleport(new Vector3(24f, 0.05f, -6f), Quaternion.identity);
        yield return Real(0.3f);
        keepAlive = false;
        defence.TakeHit(new Hit(9999f, 0f, Vector3.forward, false, true, player.transform.position + Vector3.forward));
        Judge("the player falls", defence.IsDead);
        yield return Until(() => !defence.IsDead, 8f);
        yield return Real(0.5f);
        keepAlive = true;
        float fromFire = Vector3.Distance(player.transform.position, rest.transform.position);
        Judge("a fall returns the player to the rest fire with nothing lost", !defence.IsDead && fromFire < 3f && upgrades.Points == shards && RunState.Unlocked.Count == learned && combat.Weapon.Name == weapon && element.Kit.Name == held,
            $"{fromFire:0.0} m from the fire, Shards {shards} to {upgrades.Points}, marks {learned} to {RunState.Unlocked.Count}, {combat.Weapon.Name}, {element.Kit.Name}");

        // 11. Through the door into Sun City, and back by the pause menu.
        var cityDoor = Find<SceneDoor>(door => door.LeadsTo == "SunCity");
        yield return Use(cityDoor, 1.2f);
        yield return Until(() => SceneManager.GetActiveScene().name == "SunCity", 20f);
        yield return Real(1.5f);
        FindPlayer();
        Judge("the door leads into Sun City carrying the build", SceneManager.GetActiveScene().name == "SunCity" && combat.Weapon.Name == weapon && element.Kit != null && element.Kit.Name == held && RunState.Unlocked.Count == learned,
            $"scene {SceneManager.GetActiveScene().name}, {combat.Weapon.Name}, {(element.Kit != null ? element.Kit.Name : "none")}, marks {RunState.Unlocked.Count}");
        yield return ReadAll();
        yield return Shot("pg_sun_city");
        yield return Tap(Key.Escape);
        menu = PauseMenu.Instance;
        int homeAt = -1;
        for (int i = 0; i < menu.Choices.Count; i++) if (menu.Choices.At(i) == PauseEntry.ProvingGround) homeAt = i;
        Judge("Sun City's pause menu offers the way back", homeAt >= 0, $"entries {menu.Choices.Count}");
        for (int i = 0; i < homeAt; i++) yield return Tap(Key.S);
        yield return Shot("pg_pause_return");
        yield return Tap(Key.E);
        yield return Until(() => SceneManager.GetActiveScene().name == PlaytestMode.ProvingGroundScene, 20f);
        yield return Real(1.0f);
        FindPlayer();
        Judge("back in the Proving Ground with the build and no title again", SceneManager.GetActiveScene().name == PlaytestMode.ProvingGroundScene && !GuidePage.Showing && RunState.Unlocked.Count == learned && combat.Weapon.Name == weapon,
            $"scene {SceneManager.GetActiveScene().name}, marks {RunState.Unlocked.Count}, {combat.Weapon.Name}");

        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
