using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode check of the Great Hammer's slams in the Proving Ground: the guide closed with Escape and time shown to run;
// the Great Hammer taken; for each element (AgentScripts/slam_only.txt may name some, commas between) the element taken,
// two castle enemies held where the recipe's two struck stood (about 1.1 m either side of the impact, 0.5 m past it)
// and one held outside the disc, the infused strike played with Left Ctrl + J, and ONE still at the element's peak beat
// (the Blender sheet slam_all.png's times; Lightning also 0.10 s before the head lands). Checks that the slam look was
// drawn (not the arc's), that the per-enemy pieces went on the struck only, and that no glow ring was played. Stills
// review/shots/current/slam/slam_<element>.png. Filming the whole slam is a night job (scratchpad NIGHT_FILMS.md).
//   bash AgentScripts/runprobe.sh ProvingGround SlamProbe 160
public static class SlamProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Slam Probe").AddComponent<SlamProbeRunner>();
        return "probe started";
    }
}

public class SlamProbeRunner : MonoBehaviour
{
    public const string Folder = "review/shots/current/slam";
    // AgentScripts/slam_shape.txt may name another strike shape (spin, thrust, dash, lash): its weapon is taken, the
    // enemies held where its recipe stood them (turned to the player's way), and each still taken at the recipe's third
    // beat (its strip's), into review/shots/current/<shape>/<shape>_<element>.png.
    private string shape = "slam";
    private string folder = Folder;
    private Dictionary<string, float> beats = new Dictionary<string, float>();
    private List<Vector3> recipeBodies;

    private static string WeaponOf(string shape) =>
        shape == "spin" ? Weapons.ScytheName : shape == "thrust" ? Weapons.GreatSpearName : shape == "dash" ? Weapons.TwinKnivesName
        : shape == "lash" ? Weapons.ChainSickleName : shape == "shot" ? Weapons.GreatBowName : Weapons.GreatHammerName;

    // The recipe's bodies about its player, turned so the player faces north, and each look's third strip beat.
    private void ReadRecipe()
    {
        var text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/_Project/Art/VFX/Combos/{shape}_recipe.json");
        if (text == null) return;
        var looks = (List<object>)((Dictionary<string, object>)Clover.EditorTools.ReactionRecipe.ReadJson(text.text))["looks"];
        foreach (Dictionary<string, object> look in looks)
        {
            string element = (string)look["element"];
            if (look.TryGetValue("strip", out var stripValue) && stripValue is List<object> strip && strip.Count > 2)
                beats[element] = System.Convert.ToSingle(((List<object>)strip[2])[0]);
            if (recipeBodies != null) continue;
            Vector3 way = look.TryGetValue("lane_direction", out var lane) ? V(lane) : look.TryGetValue("dash_direction", out var dash) ? V(dash) : new Vector3(-0.643f, 0f, -0.766f);
            var turn = Quaternion.Euler(0f, -Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg, 0f);
            recipeBodies = new List<Vector3>();
            foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) recipeBodies.Add(turn * V(body["position"]));
        }
    }

    private static Vector3 V(object value)
    {
        var list = (List<object>)value;
        return new Vector3(System.Convert.ToSingle(list[0]), System.Convert.ToSingle(list[1]), System.Convert.ToSingle(list[2]));
    }
    // The Blender sheet's peak beat of each, seconds after the head lands.
    private static readonly (string element, float peak)[] Peaks =
    {
        (Elements.Earth, 0.60f), (Elements.Ice, 0.60f), (Elements.Fire, 0.30f), (Elements.Water, 0.34f), (Elements.Lightning, 0.10f), (Elements.Metal, 0.52f),
        (Elements.Life, 0.30f), (Elements.Dark, 0.45f), (Elements.Poison, 0.32f), (Elements.Light, 0.32f), (Elements.Wind, 0.42f),
    };

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerSpells spells;
    private PlayerElement element;
    private PlayerDefence defence;
    private readonly List<Enemy> held = new List<Enemy>();
    private readonly List<Vector3> spots = new List<Vector3>();
    private int ok, wrong;

    private static void Keys(params Key[] down) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(down));
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

    private IEnumerator Game(float seconds)
    {
        float end = Time.time + seconds;
        float give = Time.unscaledTime + seconds + 3f;
        while (Time.time < end && Time.unscaledTime < give) yield return null;
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
        ScreenCapture.CaptureScreenshot($"{folder}/{name}.png");
        yield return null;
        yield return null;
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    // The held enemies stay on their spots, facing the player, whatever they would do.
    private void LateUpdate()
    {
        if (player == null) return;
        for (int i = 0; i < held.Count; i++)
        {
            if (held[i] == null) continue;
            Vector3 to = player.transform.position - spots[i];
            to.y = 0f;
            held[i].transform.SetPositionAndRotation(spots[i], to.sqrMagnitude > 0.01f ? Quaternion.LookRotation(to) : Quaternion.identity);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        spells = player.GetComponent<PlayerSpells>();
        element = player.GetComponent<PlayerElement>();
        defence = player.GetComponent<PlayerDefence>();
        if (File.Exists("AgentScripts/slam_shape.txt")) shape = File.ReadAllText("AgentScripts/slam_shape.txt").Trim().ToLowerInvariant();
        if (shape.Length == 0) shape = "slam";
        folder = shape == "slam" ? Folder : $"review/shots/current/{shape}";
        if (shape != "slam") ReadRecipe();
        Directory.CreateDirectory(folder);
        var only = File.Exists("AgentScripts/slam_only.txt")
            ? new HashSet<string>(File.ReadAllText("AgentScripts/slam_only.txt").Split(',').Select(s => s.Trim().ToLowerInvariant()).Where(s => s.Length > 0))
            : new HashSet<string>();

        bool alone = only.Remove("alone");
        string tag = alone ? "_alone" : "";
        yield return Until(() => GuidePage.Showing, 5f);
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        float before = Time.time;
        yield return Real(0.5f);
        Judge("the guide is closed and time runs", !GuidePage.Showing && Time.time > before, $"game time {before:0.00} -> {Time.time:0.00}");

        // The castle pen's people, held where the recipe's bodies stood.
        var castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Exclude).FirstOrDefault(pen => pen.Land == "The castle");
        var which = new List<int>();
        for (int i = 0; i < castle.Enemies.Length && which.Count < 3; i++)
            if (castle.Enemies[i] != null && !castle.Enemies[i].Kind.IsBoss) which.Add(i);
        castle.Release(which.ToArray());
        yield return Real(0.3f);
        // Where the player arrives: open, lit floor that the camera frames as it does in play.
        Vector3 stand = player.transform.position;
        // The swing steps in 0.6 m before the head lands 2.5 m ahead.
        Vector3 impact = stand + Vector3.forward * 3.1f;
        held.AddRange(which.Select(i => castle.Enemies[i]));
        if (shape == "slam")
        {
            spots.Add(impact + new Vector3(-1.05f, 0f, 0.57f));
            spots.Add(impact + new Vector3(1.15f, 0f, 0.47f));
            spots.Add(impact + new Vector3(-2.6f, 0f, 3.22f));
        }
        else if (recipeBodies != null)
            for (int i = 0; i < recipeBodies.Count && i < held.Count; i++) spots.Add(stand + (shape == "spin" ? Quaternion.identity : Quaternion.Euler(0f, 60f, 0f)) * recipeBodies[i]);
        while (held.Count > spots.Count) held.RemoveAt(held.Count - 1);
        foreach (var enemy in held)
        {
            enemy.Health.Maximum = 100000f;
            enemy.Health.Fill();
        }

        combat.Arm(WeaponOf(shape));
        yield return null;
        // Straight ahead, as the recipe's player struck: no turn toward the nearer of the two.
        combat.Weapon.AimConeDegrees = 0.5f;
        Judge($"the {WeaponOf(shape)} is held", combat.Weapon != null && combat.Weapon.Name == WeaponOf(shape));

        foreach (var (name, peak) in Peaks)
        {
            if (only.Count > 0 && !only.Contains(name.ToLowerInvariant())) continue;
            element.Take(ElementKit.ByName(name, false));
            yield return Real(0.6f);
            combat.Weapon.AimConeDegrees = 0.5f;
            int index = element.Info.Index;
            int built = System.Array.IndexOf(ElementMoves.Shapes, shape);
            Judge($"{name}: its {shape} is built", ElementMoves.HasShape(built, index), $"lead {ElementMoves.ShapeLead(built, index):0.00} s");
            // A lane or a dash is turned across the screen, as the strips turned it, so what runs down it is not hidden behind the bodies.
            var facingWay = shape == "thrust" || shape == "dash" || shape == "lash" || shape == "shot" ? Quaternion.Euler(0f, 60f, 0f) : Quaternion.identity;
            player.Teleport(stand, facingWay);
            // Fresh each time: nothing carried over from the last element (no reaction between two strikes), and none
            // staggered, so no finish mark is drawn over the look.
            castle.Release(which.ToArray());
            // "alone" in the filter: the slam on empty ground, no one held.
            if (alone) castle.Clear();
            foreach (var enemy in held)
            {
                enemy.Health.Maximum = 100000f;
                enemy.Health.Fill();
                enemy.Stagger.Maximum = 100000f;
            }
            yield return Real(0.5f);
            spells.Cooldowns.ResetAll();
            combat.Mana.Gain(100f);
            player.Stamina.Fill();
            defence.UntouchableUntil = Time.time + 6f;
            int strikes = combat.InfusedStrikes, drawn = StrikeArt.Played;
            bool lightning = name == Elements.Lightning;
            for (int attempt = 0; attempt < 3 && combat.InfusedStrikes == strikes && !combat.InfusedNow; attempt++)
            {
                Keys(Key.LeftCtrl);
                yield return null;
                yield return null;
                Keys(Key.LeftCtrl, Key.J);
                yield return null;
                yield return null;
                Keys(Key.LeftCtrl);
                yield return null;
                Keys();
                yield return Until(() => combat.InfusedNow, 1f);
            }
            if (lightning)
            {
                yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Windup && combat.Combo.Current.Windup - combat.Combo.Elapsed <= 0.10f, 1.5f);
                if (shape == "slam") yield return Still($"slam_{name.ToLowerInvariant()}_before{tag}");
            }
            yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1.5f);
            if (!combat.InfusedNow)
            {
                Judge($"{name}: the infused strike began", false, $"{defence.LastCallout}, mana {combat.Mana.Current:0}, check {spells.CheckStrike(false, out _, out _)}");
                continue;
            }
            Vector3 landed = player.transform.position + player.transform.forward * combat.Combo.Current.Reach;
            // The shot's beats are counted from the arrow stopping in its target, not from the loosing.
            if (shape == "shot")
            {
                int impactsWere = combat.ShotImpacts;
                yield return Until(() => combat.ShotImpacts > impactsWere, 1.5f);
                Say($"{name}: shot impacts +{combat.ShotImpacts - impactsWere}, misses {combat.ShotMisses}, at {Vector3.Distance(player.transform.position, combat.LastShotImpact):0.0} m");
            }
            yield return Game(shape == "slam" ? peak : beats.TryGetValue(name.ToLowerInvariant(), out float beat) ? Mathf.Max(0.02f, beat) : 0.2f);
            yield return Still($"{shape}_{name.ToLowerInvariant()}{tag}");
            yield return Real(0.2f);
            int shapeIndex = System.Array.IndexOf(ElementMoves.Shapes, shape);
            int[] reached = held.Select(enemy => ElementLooks.IsPlaying(ElementLooks.Find(ElementMoves.ShapeKey(shapeIndex, index, true)), enemy.transform) ? 1 : 0).ToArray();
            yield return Real(1.6f);
            Judge($"{name}: the slam is drawn where the head landed, not the arc's look", combat.SlamDrawn || StrikeArt.Played > drawn,
                $"strikes +{combat.InfusedStrikes - strikes}, landed {Vector3.Distance(landed, impact):0.00} m from the planned spot, per-enemy look on {string.Join("/", reached)} (near, near, far)");
            if (!alone && shape == "slam") Judge($"{name}: nothing per enemy on the one outside the disc", reached.Length < 3 || reached[2] == 0);
            yield return Until(() => !combat.IsAttacking, 2f);
        }
        castle.Clear();
        Say($"summary: {ok} OK, {wrong} WRONG");
        Say("done");
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
