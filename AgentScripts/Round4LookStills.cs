using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Magic;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// One check still of each of the 26 round-4 redesigns as built in Unity (the day rule: stills only, films at night),
// against its strip (staging/blender/2026-11-22_round4/strip_rd_<slug>.png, the game camera's top row). In the Proving
// Ground four castle soldiers (the stand-ins' size) are let out of their pens and stood where the look's recipe stood its
// bodies about a spot on the open floor, facing the player, who stands where the recipe's player stood, facing the main
// one. Each look is set off through the game: the reactions by the game's own look player (ElementLooks.PlayReaction),
// Static and Marked laid as afflictions, Gust, Glare and Flung Blade cast from the player's slots, the surges lit on F
// with Mana given. At the strip's beat time stops and a camera of the probe's own, the recipe's game camera (12.7 m off,
// 55 degrees down, 40 degree field of view, looking the way the strip's does), takes review/shots/current/round4_looks/
// <slug>.png. AgentScripts/round4_looks_only.txt may hold slugs (rd_ dropped), comma separated, to take only those.
// The guide is closed with Escape and time shown to run first; progress is sandboxed by runprobe.sh.
//   bash AgentScripts/runprobe.sh ProvingGround Round4LookStills 120
public static class Round4LookStills
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Round 4 Look Stills").AddComponent<Round4LookStillsRunner>();
        return "probe started";
    }
}

public class Round4LookStillsRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/round4_looks";
    private static readonly Vector3 Spot = new Vector3(28f, 0f, 0f);

    private enum How { Reaction, Affliction, Spell, Surge }

    // slug, how it is set off, the element, the strip's beat (seconds from t 0) it is taken at.
    private static readonly (string slug, How how, string element, float beat)[] Looks =
    {
        ("rime", How.Reaction, "", 1.0f), ("quickening", How.Reaction, "", 1.0f), ("diamond_ring", How.Reaction, "", 1.05f),
        ("shut_in_the_dark", How.Reaction, "", 0.95f), ("carried_embers", How.Reaction, "", 0.8f), ("winter_seed", How.Reaction, "", 0.85f),
        ("smoulder", How.Reaction, "", 0.75f), ("thermal_shock", How.Reaction, "", 0.8f), ("logan_stone", How.Reaction, "", 1.05f),
        ("chokedamp", How.Reaction, "", 0.9f), ("breath_taken", How.Reaction, "", 0.85f), ("glint", How.Reaction, "", 0.9f),
        ("kindle", How.Surge, Elements.Fire, 1.1f), ("static", How.Affliction, Elements.Lightning, 0.9f), ("overdrive", How.Surge, Elements.Lightning, 0.85f),
        ("gust", How.Spell, Elements.Wind, 0.4f), ("tailwind", How.Surge, Elements.Wind, 0.95f), ("wellspring", How.Surge, Elements.Water, 0.5f),
        ("bedrock", How.Surge, Elements.Earth, 1.1f), ("flung_blade", How.Spell, Elements.Metal, 0.75f), ("regrowth", How.Surge, Elements.Life, 0.95f),
        ("marked", How.Affliction, Elements.Light, 0.8f), ("glare", How.Spell, Elements.Light, 0.3f), ("clarity", How.Surge, Elements.Light, 1.0f),
        ("fever", How.Surge, Elements.Poison, 1.0f), ("hunger", How.Surge, Elements.Dark, 0.85f),
    };

    private PlayerController player;
    private PlayerDefence defence;
    private PlayerCombat combat;
    private PlayerElement element;
    private PlayerSpells spells;
    private PlayerBuild build;
    private readonly List<Enemy> row = new List<Enemy>();
    private readonly List<Vector3> spots = new List<Vector3>();
    private Vector3 playerAt;
    private readonly Dictionary<string, Dictionary<string, object>> recipes = new Dictionary<string, Dictionary<string, object>>();

    private static void Say(string line) => Debug.Log("[Probe] " + line);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private static IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        foreach (var enemy in row)
        {
            if (enemy == null || enemy.IsDead) continue;
            enemy.Health.Heal(1000f);
            enemy.Brain.AggroRange = 0f;
        }
    }

    // Held where the recipe stood them, facing the player, whatever a look shoves them by.
    private void LateUpdate()
    {
        for (int i = 0; i < row.Count && i < spots.Count; i++)
        {
            if (row[i] == null) continue;
            Vector3 toward = playerAt - spots[i];
            toward.y = 0f;
            row[i].transform.SetPositionAndRotation(spots[i], toward.sqrMagnitude > 0.01f ? Quaternion.LookRotation(toward) : Quaternion.identity);
        }
    }

    private static Vector3 Vec(object value)
    {
        if (value is List<object> list && list.Count >= 3) return new Vector3((float)(double)list[0], (float)(double)list[1], (float)(double)list[2]);
        return Vector3.zero;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        float waited = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        int guard = 0;
        while (GuidePage.Showing && guard++ < 20) yield return Tap(Key.Escape);
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.3f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames); sandboxed {Progress.Sandboxed}; art {ElementArt.Ready}");

        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        combat = player.GetComponent<PlayerCombat>();
        element = player.GetComponent<PlayerElement>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        Directory.CreateDirectory(Folder);
        foreach (string path in Clover.EditorTools.ElementLookBuilder.RdRecipePaths)
        {
            var root = (Dictionary<string, object>)Clover.EditorTools.ReactionRecipe.ReadJson(File.ReadAllText(path));
            foreach (Dictionary<string, object> look in (List<object>)root["looks"]) recipes[((string)look["slug"]).Substring(3)] = look;
        }

        // Four soldiers, the stand-ins' size, let out of the pens.
        var pens = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include);
        foreach (var pen in pens)
        {
            var which = new List<int>();
            for (int i = 0; i < pen.Enemies.Length; i++)
                if (pen.Enemies[i] != null && pen.Enemies[i].Kind.Name == EnemyKind.SoldierName) which.Add(i);
            pen.Release(which.ToArray());
        }
        yield return Real(0.5f);
        row.AddRange(pens.SelectMany(p => p.Enemies).Where(e => e != null && e.Kind.Name == EnemyKind.SoldierName).Take(4));
        if (row.Count < 4)
        {
            foreach (var pen in pens) pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            yield return Real(0.5f);
            row.AddRange(pens.SelectMany(p => p.Enemies).Where(e => e != null && !row.Contains(e) && e.Kind.Name != EnemyKind.ChampionName).Take(4 - row.Count));
        }
        Say($"stand-ins: {string.Join(", ", row.Select(e => e.Kind.Name))}");

        string only = File.Exists("AgentScripts/round4_looks_only.txt") ? File.ReadAllText("AgentScripts/round4_looks_only.txt").Trim() : "";
        var chosen = Looks.Where(l => only.Length == 0 || only.Split(',').Select(s => s.Trim()).Contains(l.slug)).ToArray();
        foreach (var look in chosen)
        {
            if (!recipes.TryGetValue(look.slug, out var recipe)) { Say($"{look.slug}: NO RECIPE"); continue; }
            yield return Take(look, recipe);
        }
        foreach (var enemy in row) if (enemy != null) enemy.Afflictions.Clear();
        if (build != null) build.Attune(Elements.Fire, "");
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say("done");
        Destroy(gameObject);
    }

    private IEnumerator Take((string slug, How how, string element, float beat) look, Dictionary<string, object> recipe)
    {
        // The recipe's bodies and player about the spot; with fewer bodies the rest stand well off.
        var bodies = ((List<object>)recipe["bodies"]).Cast<Dictionary<string, object>>().Select(b => Vec(b["position"])).ToList();
        playerAt = Spot + Vec(((Dictionary<string, object>)recipe["player"])["position"]);
        spots.Clear();
        for (int i = 0; i < row.Count; i++) spots.Add(i < bodies.Count ? Spot + bodies[i] : Spot + new Vector3(-10f - 3f * i, 0f, 8f));
        foreach (var enemy in row) if (enemy != null) enemy.Afflictions.Clear();
        // The surge, the spell and the affliction are each the element's alone.
        if (!string.IsNullOrEmpty(look.element) && (element.Info == null || element.Info.Name != look.element || element.Mixed)) build.Attune(look.element, "");
        Vector3 main = bodies.Count > 0 ? Spot + bodies[0] : Spot;
        Vector3 facing = main - playerAt;
        facing.y = 0f;
        player.Teleport(playerAt, Quaternion.LookRotation(facing.sqrMagnitude > 0.01f ? facing : Vector3.forward));
        yield return Real(1.2f);
        while (element.SurgeActive) yield return Real(0.2f);
        float surgeWas = element.SurgeKit != null ? element.SurgeKit.SurgeSeconds : 0f;
        if (look.how == How.Surge && element.SurgeKit != null) element.SurgeKit.SurgeSeconds = 3f;
        combat.Mana.Gain(100f);
        yield return null;
        bool setOff = false;
        float began = Time.time;
        switch (look.how)
        {
            case How.Reaction:
            {
                var body = row[0].transform;
                setOff = ElementLooks.PlayReaction(look.slug, body, body.position - player.transform.position, player.transform);
                break;
            }
            case How.Affliction:
            {
                var info = Elements.ByNameOrFire(look.element);
                setOff = row[0].Afflictions.Lay(info.Affliction, 3, 4f, 1f, LaySource.Spell, row[0]).Applied;
                for (int i = 1; i < row.Count && i < bodies.Count; i++) row[i].Afflictions.Lay(info.Affliction, 2, 4f, 1f, LaySource.Spell, row[i]);
                break;
            }
            case How.Spell:
            {
                int slot = spells.Slots.SlotOf(SpellSlots.BaseId(look.element));
                setOff = slot >= 0 ? spells.TryCast(slot) : element.CastBase(look.element);
                break;
            }
            case How.Surge:
                yield return Tap(Key.F);
                float until = Time.time + 2f;
                while (!element.SurgeActive && Time.time < until) yield return null;
                setOff = element.SurgeActive;
                began = Time.time;
                break;
        }
        while (Time.time - began < look.beat) yield return null;
        Time.timeScale = 0f;
        yield return null;
        // A surge is about the player: framed on them.
        if (look.how == How.Surge) Shoot(look.slug, player.transform.position, player.transform.position);
        else Shoot(look.slug, main, playerAt);
        string extra = look.how == How.Surge ? $", surge {element.SurgeActive}, fever {element.FeverLevel:0.00}, charge {element.ChargeLevel:0.00}, plates {element.BedrockPlates}, jaws {SurgeArt.JawsShown}" : "";
        Say($"{look.slug}: {(setOff ? "set off" : "NOT SET OFF")}, taken at {Time.time - began:0.00} s{extra}");
        // The surge on the player close up, standing, then mid-swing, so what the bones carry is seen following the arms.
        if (look.how == How.Surge) Shoot(look.slug + "_close_idle", player.transform.position, player.transform.position, 5f);
        yield return null;
        Time.timeScale = 1f;
        if (look.how == How.Surge)
        {
            yield return Tap(Key.J);
            float swing = Time.time + 0.18f;
            while (Time.time < swing) yield return null;
            Time.timeScale = 0f;
            yield return null;
            Shoot(look.slug + "_close_swing", player.transform.position, player.transform.position, 5f);
            yield return null;
            Time.timeScale = 1f;
        }
        if (look.how == How.Surge && element.SurgeKit != null) element.SurgeKit.SurgeSeconds = surgeWas;
        yield return Real(look.how == How.Surge ? 3.5f : 1.6f);
    }

    // The recipe's game camera: 12.7 m off, 55 degrees down, a 40 degree view, looking the strip's way (along -z), at the
    // main body a quarter of the way toward the player.
    private static void Shoot(string name, Vector3 main, Vector3 playerAt, float distance = 12.7f)
    {
        Vector3 looking = Quaternion.Euler(55f, 180f, 0f) * Vector3.forward;
        Vector3 centre = Vector3.Lerp(main, playerAt, 0.25f) + Vector3.up * 0.6f;
        var holder = new GameObject("Round 4 Camera");
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 40f;
        camera.nearClipPlane = 0.1f;
        holder.transform.position = centre - looking * distance;
        holder.transform.rotation = Quaternion.LookRotation(looking);
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes($"{Folder}/{name}.png", picture.EncodeToPNG());
        camera.targetTexture = null;
        Destroy(target);
        Destroy(picture);
        Destroy(holder);
    }
}
