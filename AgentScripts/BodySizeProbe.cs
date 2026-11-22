using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The standing size check for effects on bodies (docs/design/feedback/2026-11-20_round4_early_notes.md, "Effects on small
// and large enemies"): in the Proving Ground it stands a small enemy (the Knife Acolyte), a medium one (the Temple
// Soldier), a large one (the Gate Captain) and the largest the ground holds (the Champion of the Gate; no god's form can
// be let out here) in a row in the castle's pen, facing the camera and held still, then puts each chosen effect on all
// four at once and, with time stopped at its moments, takes a close picture of each body framed to its own size and the
// game camera's view of the row. Effects: afflictions laid by the game (Chill, Burn, Seeded, Sundered...), a strike's
// pieces on the struck body (e3_strike_<element>@b) and reactions played as the game plays them (rime, anvil_stone...).
// AgentScripts/body_size_only.txt may hold effect names with commas between to film only those; empty films the
// standard set. Frames: review/shots/current/size/<effect>_<moment>_<body>.png and <effect>_<moment>_view.png;
// python AgentScripts/body_size_sheets.py </dev/null lays each effect's moment out as one sheet in review/shots/current/size/sheets.
//   bash AgentScripts/runprobe.sh ProvingGround BodySizeProbe 60
public static class BodySizeProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Body Size Probe").AddComponent<BodySizeProbeRunner>();
        return "probe started";
    }
}

public class BodySizeProbeRunner : MonoBehaviour
{
    private enum How { Affliction, Strike, Reaction }

    private struct Effect
    {
        public string Name;
        public How How;
        public AfflictionKind Kind;
        public int Depth;
        public string Slug;
        /// <summary>Seconds after it is put on that each picture is taken.</summary>
        public float[] Moments;
    }

    private static readonly Effect[] Standard =
    {
        new Effect { Name = "chill", How = How.Affliction, Kind = AfflictionKind.Chill, Depth = 3, Moments = new[] { 1.6f } },
        new Effect { Name = "burning", How = How.Affliction, Kind = AfflictionKind.Burn, Depth = 3, Moments = new[] { 1.6f } },
        new Effect { Name = "seeded", How = How.Affliction, Kind = AfflictionKind.Seeded, Depth = 3, Moments = new[] { 1.6f } },
        new Effect { Name = "sundered", How = How.Affliction, Kind = AfflictionKind.Sundered, Depth = 3, Moments = new[] { 1.6f } },
        new Effect { Name = "poisoned", How = How.Affliction, Kind = AfflictionKind.Poison, Depth = 3, Moments = new[] { 1.6f } },
        new Effect { Name = "dark_strike", How = How.Strike, Slug = "e3_strike_dark@b", Moments = new[] { 0.35f, 0.75f } },
        new Effect { Name = "poison_strike", How = How.Strike, Slug = "e3_strike_poison@b", Moments = new[] { 0.45f, 1.0f } },
        new Effect { Name = "rime", How = How.Reaction, Slug = "rime", Moments = new[] { 0.6f, 1.4f } },
        new Effect { Name = "anvil_stone", How = How.Reaction, Slug = "anvil_stone", Moments = new[] { 0.6f, 1.3f } },
        new Effect { Name = "live_iron", How = How.Reaction, Slug = "live_iron", Moments = new[] { 1.0f } },
        new Effect { Name = "iron_thorns", How = How.Reaction, Slug = "iron_thorns", Moments = new[] { 0.5f, 1.45f } },
        new Effect { Name = "golden_feathers", How = How.Reaction, Slug = "golden_feathers", Moments = new[] { 1.55f } },
        new Effect { Name = "frost_heave", How = How.Reaction, Slug = "frost_heave", Moments = new[] { 0.8f } },
        new Effect { Name = "stone_rise", How = How.Strike, Slug = "e3_stone_rise@b", Moments = new[] { 0.6f } },
        new Effect { Name = "briar_patch", How = How.Strike, Slug = "e3_briar_patch", Moments = new[] { 1.0f } },
    };

    private const string Folder = "review/shots/current/size";
    private PlayerController player;
    private PlayerDefence defence;
    private readonly List<Enemy> row = new List<Enemy>();
    private readonly List<string> names = new List<string>();
    private readonly List<Vector3> spots = new List<Vector3>();

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
        for (int i = 0; i < row.Count; i++)
        {
            var enemy = row[i];
            if (enemy == null || enemy.IsDead) continue;
            enemy.Health.Heal(1000f);
            enemy.Brain.AggroRange = 0f;
        }
    }

    // Held on its spot facing the camera, whatever an effect shoves it by.
    private void LateUpdate()
    {
        for (int i = 0; i < row.Count; i++)
            if (row[i] != null) row[i].transform.SetPositionAndRotation(spots[i], Quaternion.Euler(0f, 180f, 0f));
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
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        Directory.CreateDirectory(Folder);
        var wanted = new[] { (EnemyKind.AcolyteName, "small"), (EnemyKind.SoldierName, "medium"), (EnemyKind.CaptainName, "large"), (EnemyKind.ChampionName, "largest") };
        var pens = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include);
        foreach (var pen in pens)
        {
            var which = new List<int>();
            for (int i = 0; i < pen.Enemies.Length; i++)
                if (pen.Enemies[i] != null && wanted.Any(w => w.Item1 == pen.Enemies[i].Kind.Name)) which.Add(i);
            pen.Release(which.ToArray());
        }
        yield return Real(0.5f);
        Vector3 centre = new Vector3(-14f, 0.05f, 30f);
        for (int w = 0; w < wanted.Length; w++)
        {
            var enemy = pens.SelectMany(p => p.Enemies).FirstOrDefault(e => e != null && e.Kind.Name == wanted[w].Item1);
            if (enemy == null) { Say($"{wanted[w].Item1} MISSING"); continue; }
            row.Add(enemy);
            names.Add(wanted[w].Item2);
            spots.Add(centre + new Vector3(-6.75f + w * 4.5f, 0f, 0f));
        }
        // Well back from the row, so no one in it is drawn into a fight (the row is pictured by cameras of the probe's own).
        player.Teleport(centre + new Vector3(0f, 0f, -8.5f), Quaternion.identity);
        yield return Real(1.2f);
        for (int i = 0; i < row.Count; i++)
        {
            int slot = BodyAnchors.Slot(row[i].transform);
            BodyAnchors.TryAnchors(slot, out var a);
            BodyAnchors.TryPosed(slot, out var posed);
            Say($"{names[i]} {row[i].Kind.Name}: waist {a.Waist:0.00} chest {a.Chest:0.00} head {a.Head:0.00} crown {a.Crown:0.00} girth {a.Girth:0.00} size {BodyAnchors.SizeOf(a):0.00}; standing now waist {posed.Anchors.Waist:0.00} chest {posed.Anchors.Chest:0.00} head {posed.Anchors.Head:0.00} fold {BodyAnchors.FoldOf(a, posed.Anchors):0.00}");
        }

        string only = File.Exists("AgentScripts/body_size_only.txt") ? File.ReadAllText("AgentScripts/body_size_only.txt").Trim() : "";
        var chosen = string.IsNullOrEmpty(only) ? Standard : Standard.Where(e => only.Split(',').Select(s => s.Trim()).Contains(e.Name)).ToArray();
        foreach (var effect in chosen)
        {
            foreach (var enemy in row) enemy.Afflictions.Clear();
            yield return Real(0.9f);
            float began = Time.time;
            bool played = true;
            for (int i = 0; i < row.Count; i++)
            {
                var body = row[i].transform;
                Vector3 away = body.position - player.transform.position;
                switch (effect.How)
                {
                    case How.Affliction:
                        row[i].Afflictions.Lay(effect.Kind, effect.Depth, 30f, 1f, LaySource.Spell, row[i]);
                        break;
                    case How.Strike:
                        int look = ElementLooks.Find(effect.Slug);
                        played &= look >= 0 && ElementLooks.Play(look, body, Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg);
                        break;
                    case How.Reaction:
                        played &= ElementLooks.PlayReaction(effect.Slug, body, away, player.transform);
                        break;
                }
            }
            Say($"{effect.Name}: put on {(played ? "all" : "NOT ALL")}");
            for (int m = 0; m < effect.Moments.Length; m++)
            {
                while (Time.time - began < effect.Moments[m]) yield return null;
                Time.timeScale = 0f;
                yield return null;
                for (int i = 0; i < row.Count; i++) Close($"{effect.Name}_{m + 1}_{names[i]}", row[i]);
                Row($"{effect.Name}_{m + 1}_view");
                yield return null;
                yield return null;
                Time.timeScale = 1f;
            }
        }
        foreach (var enemy in row) enemy.Afflictions.Clear();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say("done");
        Destroy(gameObject);
    }

    // The way the game's camera looks, so what is turned to face it (Sundered's dagger) is seen as the player sees it.
    private static Vector3 Looking()
    {
        var main = Camera.main;
        return main != null ? main.transform.forward : Quaternion.Euler(55f, 0f, 0f) * Vector3.forward;
    }

    // A camera of its own looking at the body the way the game's camera does, framed to the body's own height so every
    // body fills its picture alike and what sits on it can be compared.
    private static void Close(string name, Enemy enemy)
    {
        float tall = 2f;
        if (BodyAnchors.TryAnchors(BodyAnchors.Slot(enemy.transform), out var a)) tall = a.Crown;
        Vector3 centre = enemy.transform.position + Vector3.up * (tall * 0.5f);
        Shoot(name, centre, Looking(), tall * 2.8f, 560, 640);
    }

    // The whole row as the game's camera would frame it standing over the row's middle.
    private void Row(string name)
    {
        Vector3 centre = (spots[0] + spots[spots.Count - 1]) * 0.5f + Vector3.up * 1.2f;
        Shoot(name, centre, Looking(), 17f, 1280, 640);
    }

    private static void Shoot(string name, Vector3 centre, Vector3 looking, float distance, int width, int height)
    {
        var holder = new GameObject("Size Camera");
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 30f;
        camera.nearClipPlane = 0.05f;
        holder.transform.position = centre - looking * distance;
        holder.transform.LookAt(centre);
        var target = new RenderTexture(width, height, 24);
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
