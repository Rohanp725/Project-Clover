using System;
using System.Collections;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The standins merge's pieces that were tested only in code, played in the Castle with its real Temple Soldier and Gate
// Captain (the Castle is the Lightning land, so its elite lays Static on the player; the Proving Ground is in no land):
//   windup  - a soldier's wind-up: AfflictionSet.WindingUp / WindupFor / StrikeIn while its brain shows the attack;
//   landing - the blow about to land: To the Blade cast with its Master as the soldier's blow lands counts as a perfect dodge;
//   shove   - Gust's push on a soldier is a shove moment that ends a condition a shove ends; a pull's small drag is not;
//   guard   - Snapped Blade: a plain held guard against a soldier carrying coldshort.snapped is a perfect guard (a plain
//             held guard without it only blocks);
//   badge   - the Gate Captain's blow lays the land's affliction on the player and the HUD's mark shows its name and seconds.
// The guide closed with Escape and time shown to run first. One still each: review/shots/current/standins/<piece>.png.
//   bash AgentScripts/runprobe.sh Castle StandinsProbe 90
public static class StandinsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Standins Probe").AddComponent<StandinsProbeRunner>();
        return "probe started";
    }
}

public class StandinsProbeRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/standins";
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerCombat combat;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerElement element;
    private Enemy soldier, captain;
    private Enemy[] everyone;
    private int ok, failed;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private static readonly Vector3 SoldierSpot = new Vector3(-3f, 0f, 40f);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Judge(string what, bool good, string detail)
    {
        if (good) ok++; else failed++;
        Say($"{(good ? "OK" : "FAIL")} {what}: {detail}");
    }

    private static IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private static IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private static IEnumerator Still(string name)
    {
        float scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot($"{Folder}/{name}.png");
        yield return null;
        yield return null;
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        if (player != null) player.Stamina.Fill();
        if (everyone == null) return;
        foreach (var enemy in everyone)
            if (enemy != null && !enemy.IsDead) enemy.Health.Heal(1000f);
    }

    // Only the one enemy a piece is about may fight; the rest stand.
    private void OnlyAwake(Enemy awake)
    {
        foreach (var enemy in everyone)
        {
            if (enemy == null) continue;
            enemy.Brain.AggroRange = enemy == awake ? 11f : 0f;
            if (enemy != awake && enemy.Brain.State == EnemyState.Telegraph) enemy.Brain.Reset();
        }
    }

    // The player a few metres in front of the soldier at its post, facing it, and the soldier back at its post.
    private IEnumerator FaceSoldier(float gap)
    {
        Keys();
        soldier.Brain.Reset();
        soldier.transform.SetPositionAndRotation(SoldierSpot, Quaternion.Euler(0f, 180f, 0f));
        player.Teleport(SoldierSpot + Vector3.back * gap, Quaternion.identity);
        defence.Restore();
        yield return Real(0.4f);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        int presses = 0;
        while (GuidePage.Showing && presses++ < 20) yield return Tap(Key.Escape);
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && presses++ < 40) yield return Tap(Key.E);
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.4f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        combat = player.GetComponent<PlayerCombat>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        element = player.GetComponent<PlayerElement>();
        everyone = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        soldier = everyone.Where(e => e.Kind.Name == EnemyKind.SoldierName).OrderBy(e => (e.transform.position - SoldierSpot).sqrMagnitude).FirstOrDefault();
        captain = everyone.FirstOrDefault(e => e.Kind.Name == EnemyKind.CaptainName);
        Say($"scene {gameObject.scene.name}/{player.gameObject.scene.name}: {everyone.Length} enemies, soldier {(soldier != null ? soldier.transform.position.ToString("0.0") : "MISSING")}, captain {(captain != null ? captain.transform.position.ToString("0.0") + " rank " + captain.Afflictions.Rank : "MISSING")}");
        if (soldier == null || captain == null) { Finish(); yield break; }

        // AgentScripts/standins_only.txt (comma list of pieces) runs only those.
        string only = File.Exists("AgentScripts/standins_only.txt") ? File.ReadAllText("AgentScripts/standins_only.txt").Trim() : "";
        bool Wants(string piece) => only.Length == 0 || only.Split(',').Any(p => p.Trim() == piece);
        // AgentScripts/standins_film.txt present: each piece is also filmed, 20 frames a second of game time, the whole game
        // view to review/gifs/standins/frames/<piece>/<nnn>.png (assembled by Pillow into review/gifs/standins/<piece>.gif).
        bool film = File.Exists("AgentScripts/standins_film.txt");
        if (Wants("windup")) { if (film) Roll("windup"); yield return Windup(); Cut(); }
        if (Wants("landing")) { if (film) Roll("landing"); yield return Landing(); Cut(); }
        if (Wants("shove")) { if (film) Roll("shove"); yield return Shove(); Cut(); }
        if (Wants("guard")) { if (film) Roll("guard"); yield return Guard(); Cut(); }
        if (Wants("badge")) { if (film) Roll("badge"); yield return Badge(); Cut(); }
        Finish();
    }

    // ---- The enemy's wind-up: the flag, how long it has shown it, and when its blow lands.
    private IEnumerator Windup()
    {
        OnlyAwake(soldier);
        yield return FaceSoldier(3f);
        // A fresh wind-up, caught at its first step: one already under way when the player arrives is let pass.
        yield return Until(() => !soldier.Afflictions.WindingUp, 3f);
        yield return Until(() => soldier.Afflictions.WindingUp, 8f);
        if (!soldier.Afflictions.WindingUp) { Judge("windup", false, "the soldier showed no attack in 8 s"); yield break; }
        var attack = soldier.Brain.Current;
        float firstStrikeIn = soldier.Afflictions.StrikeIn, firstFor = soldier.Afflictions.WindupFor;
        yield return Until(() => !soldier.Afflictions.WindingUp || soldier.Afflictions.WindupFor >= 0.3f, 2f);
        float midFor = soldier.Afflictions.WindupFor, midStrikeIn = soldier.Afflictions.StrikeIn;
        bool showing = soldier.Brain.State == EnemyState.Telegraph;
        yield return Still("windup");
        yield return Until(() => !soldier.Afflictions.WindingUp, 2f);
        bool ended = !soldier.Afflictions.WindingUp && soldier.Afflictions.WindupFor == float.MaxValue && soldier.Afflictions.StrikeIn == float.MaxValue;
        Judge("windup", showing && firstFor < 0.1f && midFor >= 0.28f && Mathf.Abs(firstStrikeIn - midStrikeIn - (midFor - firstFor)) < 0.12f && ended,
            $"{attack?.Name} telegraph {attack?.Telegraph:0.00} s: at its start WindupFor {firstFor:0.00} StrikeIn {firstStrikeIn:0.00}; then WindupFor {midFor:0.00} StrikeIn {midStrikeIn:0.00} (brain Telegraph {showing}); after the blow WindingUp {soldier.Afflictions.WindingUp}, cleared {ended}");
    }

    // ---- The blow about to land: To the Blade with its Master cast in the last 0.3 s counts as a perfect dodge.
    private IEnumerator Landing()
    {
        upgrades.Give(Upgrade.ById(MetalTree.ToTheBlade));
        upgrades.Give(Upgrade.ById(SpellBranch.MasterId(MetalTree.ToTheBlade)));
        build.Attune(Elements.Metal, "");
        spells.SetSlot(0, MetalTree.ToTheBlade);
        upgrades.RefreshNow();
        bool tried = false, cast = false;
        int dodges = 0, movement = 0, taken = 0;
        float castAt = 0f;
        Component found = null;
        for (int attempt = 0; attempt < 3 && !cast; attempt++)
        {
            yield return FaceSoldier(2.6f);
            spells.Cooldowns.ResetAll();
            combat.Mana.Gain(1000f);
            yield return Until(() => soldier.Afflictions.WindingUp, 8f);
            yield return Until(() => !soldier.Afflictions.WindingUp || soldier.Afflictions.StrikeIn <= 0.2f, 2f);
            if (!soldier.Afflictions.WindingUp) continue;
            found = combat.BlowAboutToLand(0.3f, out castAt);
            dodges = defence.PerfectDodges;
            movement = upgrades.MovementDodges;
            taken = defence.BlowsTaken;
            tried = true;
            cast = spells.TryCast(0);
        }
        if (!tried) { Judge("landing", false, "no wind-up reached its last 0.2 s"); yield break; }
        yield return Real(0.12f);
        yield return Still("landing");
        yield return Real(0.8f);
        Judge("landing", found == soldier && cast && upgrades.MovementDodges == movement + 1 && defence.PerfectDodges == dodges + 1 && defence.BlowsTaken == taken,
            $"BlowAboutToLand found {(found == soldier ? "the soldier" : found == null ? "none" : found.name)} {castAt:0.00} s out; To the Blade cast {cast}; MovementDodges {movement}->{upgrades.MovementDodges}, PerfectDodges {dodges}->{defence.PerfectDodges}, blows taken {taken}->{defence.BlowsTaken}");
        spells.SetSlot(0, "");
        RunState.Unlocked.Remove(MetalTree.ToTheBlade);
        RunState.Unlocked.Remove(SpellBranch.MasterId(MetalTree.ToTheBlade));
        upgrades.RefreshNow();
    }

    // ---- A shove: Gust's push ends a condition a shove ends; a pull's small drag does not.
    private IEnumerator Shove()
    {
        const string id = "probe.untilshoved";
        bool seenFix = false;
        OnlyAwake(null);
        yield return FaceSoldier(3f);
        // A second soldier beside the first, so the wind reaches two bodies: each must feel exactly one shove.
        var second = everyone.Where(e => e != soldier && e.Kind.Name == EnemyKind.SoldierName && !e.IsDead).OrderBy(e => (e.transform.position - SoldierSpot).sqrMagnitude).FirstOrDefault();
        if (second != null) { second.Brain.Reset(); second.transform.SetPositionAndRotation(SoldierSpot + Vector3.right * 1.4f, Quaternion.Euler(0f, 180f, 0f)); }
        var bodies = second != null ? new[] { soldier, second } : new[] { soldier };
        var lastMoment = typeof(Enemy).GetField("lastShoveMoment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float MomentOf(Enemy e) => (float)lastMoment.GetValue(e);
        yield return Real(0.3f);
        foreach (var body in bodies) body.Afflictions.Conditions.Set(new ConditionSpec { Id = id, Seconds = 30f, EndsWith = ConditionBlow.Shove }, player);
        Vector3 before = soldier.transform.position;
        soldier.Push(Vector3.forward * 0.3f);
        yield return Real(0.3f);
        bool keptByLight = soldier.Afflictions.Conditions.Holds(id);
        float dragged = (soldier.transform.position - before).magnitude;
        // A drag a step at a time, as Eye's carry and Lodestone's pull give it, even with a long frame's big step: never a shove.
        float dragMoment = MomentOf(soldier);
        for (int step = 0; step < 20; step++) { soldier.Push(Vector3.back * 1.2f, PushKind.Drag); yield return null; }
        yield return Real(0.4f);
        bool keptByDrag = keptByLight && soldier.Afflictions.Conditions.Holds(id) && MomentOf(soldier) == dragMoment;
        yield return FaceSoldier(3f);
        build.Attune(Elements.Wind, "");
        upgrades.RefreshNow();
        combat.Mana.Gain(1000f);
        before = soldier.transform.position;
        // The second soldier set beside the first just before the wind, its post moved there so it does not walk home.
        // Some soldiers are held in place by their own post (a sentry on a wall): the first that stays where it is set is used.
        if (second != null)
        {
            Vector3 spot = soldier.transform.position + Vector3.right * 1.3f;
            var homeField = typeof(Enemy).GetField("home", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var candidates = everyone.Where(e => e != soldier && e.Kind.Name == EnemyKind.SoldierName && !e.IsDead).OrderBy(e => (e.transform.position - SoldierSpot).sqrMagnitude).ToArray();
            second = null;
            foreach (var candidate in candidates)
            {
                candidate.Brain.Reset();
                candidate.transform.SetPositionAndRotation(spot, Quaternion.Euler(0f, 180f, 0f));
                homeField?.SetValue(candidate, spot);
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                if ((candidate.transform.position - spot).sqrMagnitude < 0.25f) { second = candidate; break; }
                Say($"soldier {candidate.name} did not stay at the spot ({candidate.transform.position:0.0}); next");
            }
            bodies = second != null ? new[] { soldier, second } : new[] { soldier };
            seenFix = true;
        }
        _ = seenFix;
        var starts = bodies.Select(b => b.transform.position).ToArray();
        var seen = bodies.Select(MomentOf).ToArray();
        var moments = new int[bodies.Length];
        bool gust = element.CastBase(Elements.Wind);
        float far = 0f;
        float end = Time.time + 1.2f;
        bool stillTaken = false;
        while (Time.time < end)
        {
            for (int b = 0; b < bodies.Length; b++)
                if (MomentOf(bodies[b]) != seen[b]) { moments[b]++; seen[b] = MomentOf(bodies[b]); }
            far = Mathf.Max(far, (soldier.transform.position - before).magnitude);
            if (!stillTaken && Time.time > end - 0.85f) { stillTaken = true; yield return Still("shove"); }
            yield return null;
        }
        bool ended = bodies.All(b => !b.Afflictions.Conditions.Holds(id));
        Judge("shove", keptByDrag && gust && ended && far > 0.75f && moments.All(m => m == 1),
            $"a 0.3 push moved it {dragged:0.00} m and the condition held {keptByLight}; 20 drag steps of 1.2 held it {keptByDrag}; Gust cast {gust}, pushed it {far:0.00} m, shove moments per body [{string.Join(",", moments)}] over {bodies.Length} bodies at {string.Join(" ", starts.Select(p => (p - player.transform.position).ToString("0.0")))} moved {string.Join(",", bodies.Select((b, i) => (b.transform.position - starts[i]).magnitude.ToString("0.00")))} m, the conditions ended {ended} ({1f / Time.smoothDeltaTime:0} fps)");
        foreach (var body in bodies) body.Afflictions.Conditions.Clear();
    }

    // ---- Snapped Blade: a plain held guard against the snapped soldier is perfect; without it, a block.
    private IEnumerator Guard()
    {
        build.Attune(Elements.Fire, "");
        upgrades.RefreshNow();
        OnlyAwake(soldier);
        int[] blocks = new int[2], perfect = new int[2], taken = new int[2];
        string[] attacks = new string[2];
        for (int round = 0; round < 2; round++)
        {
            yield return FaceSoldier(2.6f);
            if (round == 1)
                soldier.Afflictions.Conditions.Set(new ConditionSpec { Id = Clover.Player.Trees.Duo.ColdShortDuo.Snapped, Seconds = 30f,
                    Flags = ConditionFlags.DamageDealtShare | ConditionFlags.GuardMadePerfect, Scale = 0.5f }, player);
            // A blow a guard can meet: the lunge is unblockable, so a wind-up into it is let pass for the next.
            yield return Until(() => soldier.Afflictions.WindingUp && soldier.Afflictions.StrikeIn > 0.35f && soldier.Brain.Current != null && !soldier.Brain.Current.Unblockable, 12f);
            attacks[round] = soldier.Brain.Current?.Name;
            // Held well before the blow, so it is no timed guard of its own.
            Keys(Key.LeftShift);
            int b = defence.Blocks, p = defence.PerfectGuards, t = defence.BlowsTaken;
            yield return Until(() => defence.Blocks > b || defence.PerfectGuards > p || defence.BlowsTaken > t, 3f);
            if (round == 1) yield return Still("guard");
            blocks[round] = defence.Blocks - b;
            perfect[round] = defence.PerfectGuards - p;
            taken[round] = defence.BlowsTaken - t;
            yield return Real(0.3f);
            Keys();
            yield return Real(0.6f);
        }
        bool madePerfect = PlayerDefence.GuardMadePerfect(soldier);
        Judge("guard", blocks[0] == 1 && perfect[0] == 0 && perfect[1] == 1 && blocks[1] == 0 && madePerfect,
            $"plain soldier ({attacks[0]}): blocked {blocks[0]}, perfect {perfect[0]}, taken {taken[0]}; snapped soldier ({attacks[1]}): blocked {blocks[1]}, perfect {perfect[1]}, taken {taken[1]}; GuardMadePerfect {madePerfect}");
        soldier.Afflictions.Conditions.Clear();
    }

    // ---- The elite's blow lays the land's affliction; the HUD's mark shows its name and seconds.
    private IEnumerator Badge()
    {
        OnlyAwake(captain);
        soldier.Brain.Reset();
        defence.Afflictions.Clear();
        defence.Restore();
        player.Teleport(new Vector3(0f, 4f, 116.5f), Quaternion.identity);
        int laid = defence.Afflictions.Laid, taken = defence.BlowsTaken;
        yield return Until(() => defence.Afflictions.Laid > laid, 15f);
        var kind = PlayerAfflictions.OfLand(Outfits.LandOf(captain.gameObject.scene.name));
        int stacks = defence.Afflictions.Stacks(kind);
        float seconds = defence.Afflictions.SecondsLeft(kind);
        yield return Real(0.3f);
        // Out of the captain's reach, so the mark counts down without a second blow.
        OnlyAwake(null);
        captain.Brain.Reset();
        player.Teleport(new Vector3(0f, 4f, 108f), Quaternion.identity);
        yield return Real(0.4f);
        string shown = BadgeWords();
        yield return Still("badge");
        yield return Real(1.2f);
        string later = BadgeWords();
        bool reads = shown.StartsWith(Afflictions.Spec(kind).Name + " ") && shown != later;
        Judge("badge", defence.Afflictions.Laid > laid && kind != AfflictionKind.None && stacks >= 1 && reads,
            $"captain ({captain.Afflictions.Rank}) in {captain.gameObject.scene.name} -> land {Outfits.LandOf(captain.gameObject.scene.name)}: blows taken {defence.BlowsTaken - taken}, laid {kind} x{stacks} for {seconds:0.0} s; the HUD mark read \"{shown}\", 1.2 s later \"{later}\"");
        defence.Afflictions.Clear();
    }

    private string rolling;
    private int rolled;

    private void Roll(string piece)
    {
        rolling = $"review/gifs/standins/frames/{piece}";
        Directory.CreateDirectory(rolling);
        foreach (var old in Directory.GetFiles(rolling, "*.png")) File.Delete(old);
        rolled = 0;
        Time.captureFramerate = 20;
    }

    private void Cut()
    {
        if (rolling != null) Say($"filmed {Path.GetFileName(rolling)}: {rolled} frames");
        rolling = null;
        Time.captureFramerate = 0;
    }

    private void LateUpdate()
    {
        if (rolling == null) return;
        ScreenCapture.CaptureScreenshot($"{rolling}/{rolled:000}.png");
        rolled++;
    }

    private static string BadgeWords()
    {
        var marks = FindAnyObjectByType<AfflictionBadges>();
        if (marks == null) return "(no AfflictionBadges in the HUD)";
        var words = marks.GetComponentsInChildren<UnityEngine.UI.Text>(false);
        return words.Length == 0 ? "(no mark shown)" : string.Join(" | ", words.Select(w => w.text));
    }

    private void Finish()
    {
        Keys();
        if (spells != null) spells.SetSlot(0, "");
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        if (defence != null) defence.Afflictions.Clear();
        Cut();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say($"tally: {ok} OK, {failed} FAIL");
        Say("done");
        Destroy(gameObject);
    }
}
