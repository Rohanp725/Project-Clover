using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A bot that plays badly for a long time with seeded random keys, and writes down whatever breaks.
// It reads "<mode> <seed> <seconds>" from %TEMP%/soak_config.txt: mode chaos (mortal, no help), clear (kept alive,
// enemies weakened, goes round the whole run again and again) or arena (the scene it is in, every upgrade of one
// element then the other). Every line also goes to %TEMP%/soak_log.txt because the console only keeps a tail.
// It forgets saved progress before and after.
public static class SoakProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Soak Probe").AddComponent<SoakProbeRunner>();
        return "probe started";
    }
}

public class SoakProbeRunner : MonoBehaviour
{
    private string mode = "chaos";
    private int seed = 1;
    private float seconds = 120f;
    private System.Random random;
    private string logPath;

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerInteraction interaction;
    private PlayerUpgrades upgrades;
    private Enemy[] enemies = new Enemy[0];
    private Colossus colossus;
    private AreaDirector area;
    private Text emberText;
    private string sceneName = "";
    private float sceneSince;
    private int framesInScene;
    private Vector3 spawn;

    // Trouble, by kind.
    private readonly Dictionary<string, int> trouble = new Dictionary<string, int>();
    private readonly Dictionary<string, int> errors = new Dictionary<string, int>();
    private readonly List<string> errorDetail = new List<string>();
    private int warnings;
    private int spikes, loadSpikes;
    private float worstSpike;
    private readonly List<string> loops = new List<string>();

    // Watches.
    private Vector3 lastPosition;
    private float unmovedFor, suspendedFor, scaleOffFor, emberOffFor, deadFor, noPlayerFor, boxedFor, doorRefusedFor;
    private Vector3 boxAnchor;
    private readonly Dictionary<Enemy, float> strikingFor = new Dictionary<Enemy, float>();
    private readonly Dictionary<Enemy, EnemyState> strikingState = new Dictionary<Enemy, EnemyState>();
    private string colossusDoing = "";
    private float colossusFor;
    private bool wantsMove;

    // The bot's own plan.
    private readonly List<Key> held = new List<Key>();
    private float nextDecision;
    private bool releaseFrame;
    private float sidestepUntil;
    private Vector2 sidestep;
    private float progressBest = float.MaxValue;
    private float progressSince;
    private Component progressTarget;
    private float nextAssist;
    private bool restedThisLoop, wantsKatana, wantsDetour, restPlanned;
    private GodReward rewardPick;
    private int deaths, kills, sceneChanges, bought, assists, runsHome;
    private int lastAlive = -1;
    private int lastUnlocked;
    private bool lastDead;
    private bool arenaSecondHalf;
    private float gameTime;
    private float startedAt;
    private bool finished;

    private void Say(string text)
    {
        string line = "[Probe] " + text;
        Debug.Log(line);
        try { File.AppendAllText(logPath, $"{gameTime,7:0.0} {line}\n"); } catch (Exception) { }
    }

    private void Trouble(string kind, string detail)
    {
        trouble.TryGetValue(kind, out int count);
        trouble[kind] = count + 1;
        if (count == 0) Say($"TROUBLE {kind}: {detail} (scene {sceneName}, t {gameTime:0.0})");
    }

    private static void ForgetAll()
    {
        Progress.Forget(Progress.MetSponsor);
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
    }

    private void OnLog(string message, string stack, LogType type)
    {
        if (message.StartsWith("[Probe]")) return;
        if (type == LogType.Warning) { warnings++; return; }
        if (type == LogType.Log) return;
        errors.TryGetValue(message, out int count);
        errors[message] = count + 1;
        if (count == 0 && errorDetail.Count < 8)
        {
            errorDetail.Add($"{type}: {message}\n{stack}");
            try { File.AppendAllText(logPath, $"{gameTime,7:0.0} ERROR {type}: {message}\n{stack}\n"); } catch (Exception) { }
        }
    }

    private void Awake()
    {
        string temp = Environment.GetEnvironmentVariable("TEMP");
        logPath = Path.Combine(temp, "soak_log.txt");
        try
        {
            string[] parts = File.ReadAllText(Path.Combine(temp, "soak_config.txt")).Trim().Split(' ');
            mode = parts[0];
            seed = int.Parse(parts[1]);
            seconds = float.Parse(parts[2]);
        }
        catch (Exception) { }
        random = new System.Random(seed);
        try { File.WriteAllText(logPath, ""); } catch (Exception) { }
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        Application.logMessageReceived += OnLog;
        startedAt = Time.unscaledTime;
        Say($"soak start mode {mode} seed {seed} seconds {seconds} scene {SceneManager.GetActiveScene().name}");
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        ForgetAll();
    }

    private bool MenuOpen => UpgradeMenu.Instance != null && UpgradeMenu.Instance.IsOpen;
    private bool BoxOpen => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen;

    private void FindScene()
    {
        sceneName = SceneManager.GetActiveScene().name;
        sceneSince = Time.unscaledTime;
        framesInScene = 0;
        player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        interaction = player.GetComponent<PlayerInteraction>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        colossus = FindAnyObjectByType<Colossus>();
        area = FindAnyObjectByType<AreaDirector>();
        emberText = null;
        foreach (var text in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (text.text.StartsWith("Embers")) emberText = text;
        spawn = player.transform.position;
        lastPosition = spawn;
        boxAnchor = spawn;
        unmovedFor = suspendedFor = scaleOffFor = emberOffFor = deadFor = boxedFor = doorRefusedFor = 0f;
        strikingFor.Clear();
        strikingState.Clear();
        colossusDoing = "";
        colossusFor = 0f;
        lastAlive = -1;
        lastDead = false;
        rewardPick = null;
        progressTarget = null;
        sceneChanges++;
        Say($"scene {sceneName}: weapon {combat.Weapon.Name} element {element.Kit.Name} embers {RunState.Points} learned [{string.Join(",", RunState.Unlocked)}] enemies {enemies.Length} runs ended {RunState.RunsEnded}");
        if (sceneName == "Hub")
        {
            runsHome++;
            restedThisLoop = false;
            wantsKatana = random.Next(100) < 60;
            wantsDetour = random.Next(100) < 50;
            restPlanned = random.Next(100) < 70;
            GC.Collect();
            int roots = gameObject.scene.GetRootGameObjects().Length;
            int transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            string sample = $"loop {runsHome}: managed {GC.GetTotalMemory(false) / 1024} KB, DontDestroyOnLoad roots {roots}, transforms {transforms}";
            loops.Add(sample);
            Say(sample);
        }
        if (mode == "arena") SetArenaElement(arenaSecondHalf ? ElementKit.LightningName : ElementKit.FireName);
    }

    private void SetArenaElement(string name)
    {
        RunState.Unlocked.Clear();
        foreach (var upgrade in Upgrade.All)
            if (upgrade.Element == name) RunState.Unlocked.Add(upgrade.Id);
        RunState.Element = name;
        element.Take(ElementKit.ByName(name, random.Next(2) == 0));
        combat.Equip(Weapons.ByName(random.Next(3) == 0 ? Weapons.StormKatanaName : Weapons.KatanaName));
        Say($"arena element {name} weapon {combat.Weapon.Name} learned [{string.Join(",", RunState.Unlocked)}]");
    }

    private static bool Bad(Vector3 p) => float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z);

    private void Update()
    {
        if (finished) return;
        float real = Time.unscaledDeltaTime;
        gameTime += Time.deltaTime;

        if (player == null || SceneManager.GetActiveScene().name != sceneName)
        {
            FindScene();
            if (player == null)
            {
                noPlayerFor += real;
                if (noPlayerFor > 10f) Trouble("no player", "no PlayerController for 10 s after a scene change");
                return;
            }
            noPlayerFor = 0f;
        }
        framesInScene++;
        if (real > 0.1f)
        {
            if (framesInScene <= 5) loadSpikes++;
            else
            {
                spikes++;
                worstSpike = Mathf.Max(worstSpike, real);
                if (spikes <= 6) Say($"spike {real * 1000f:0} ms in {sceneName} frame {framesInScene} of scene");
            }
        }

        Watch(real);
        if (mode == "clear" && defence != null) defence.Health.Heal(100f);
        if (mode == "arena")
        {
            if (random.Next(200) == 0) combat.Mana.Gain(100f);
            if (!arenaSecondHalf && Time.unscaledTime - startedAt > seconds * 0.5f)
            {
                arenaSecondHalf = true;
                SetArenaElement(ElementKit.LightningName);
            }
        }
        if (mode == "clear") Assist();
        Play();

        if (Time.unscaledTime - startedAt > seconds) Finish();
    }

    private void Watch(float real)
    {
        Vector3 at = player.transform.position;
        bool open = MenuOpen || BoxOpen;
        if (Bad(at)) Trouble("player NaN", at.ToString());
        else if (at.y < -5f || at.y > 30f) Trouble("player out of the world", at.ToString("0.0"));

        bool dead = defence.IsDead;
        if (dead && !lastDead)
        {
            deaths++;
            Say($"death {deaths} in {sceneName} at {at:0.0}");
        }
        lastDead = dead;
        if (dead && sceneName != "CombatTest") deadFor += real; else deadFor = 0f;
        if (deadFor > 12f) Trouble("death did not end", $"dead for 12 s in {sceneName} without waking in the hub");

        if (wantsMove && !open && !dead && !player.Suspended && Time.timeScale > 0.05f)
        {
            if ((at - lastPosition).sqrMagnitude < 0.0004f) unmovedFor += Time.deltaTime;
            else { unmovedFor = 0f; lastPosition = at; }
            if (unmovedFor > 15f) { Trouble("player stuck", $"did not move for 15 s at {at:0.0} while keys said move"); unmovedFor = 0f; }
            if ((at - boxAnchor).sqrMagnitude > 0.25f) { boxAnchor = at; boxedFor = 0f; }
            else boxedFor += Time.deltaTime;
            if (boxedFor > 20f) { Trouble("player boxed in", $"stayed within half a metre of {at:0.0} for 20 s of moving keys"); boxedFor = 0f; }
        }

        if (player.Suspended && !open && !dead) suspendedFor += real; else suspendedFor = 0f;
        if (suspendedFor > 3f) { Trouble("suspended with nothing open", $"Suspended for 3 s, menu {MenuOpen} box {BoxOpen}"); suspendedFor = 0f; }

        if (!MenuOpen && Mathf.Abs(Time.timeScale - 1f) > 0.001f) scaleOffFor += real; else scaleOffFor = 0f;
        if (scaleOffFor > 6f) { Trouble("time scale stuck", $"timeScale {Time.timeScale} for 6 s outside the menu, dead {dead}"); scaleOffFor = 0f; }

        if (emberText != null && emberText.isActiveAndEnabled && emberText.text != $"Embers  {RunState.Points}") emberOffFor += real; else emberOffFor = 0f;
        if (emberOffFor > 5f) { Trouble("ember count wrong", $"HUD says '{emberText.text}' but the run holds {RunState.Points}"); emberOffFor = 0f; }

        int alive = 0;
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == null) continue;
            if (!enemy.IsDead) alive++;
            Vector3 p = enemy.transform.position;
            if (Bad(p)) Trouble("enemy NaN", enemy.name);
            else if (!enemy.IsDead && (p.y < -5f || p.y > 30f)) Trouble("enemy out of the world", $"{enemy.name} at {p:0.0} state {enemy.Brain.State}");
            var state = enemy.Brain.State;
            bool attacking = state == EnemyState.Telegraph || state == EnemyState.Strike || state == EnemyState.Recover;
            strikingState.TryGetValue(enemy, out var before);
            strikingFor.TryGetValue(enemy, out float time);
            time = attacking && before == state ? time + Time.deltaTime : 0f;
            strikingFor[enemy] = time;
            strikingState[enemy] = state;
            if (time > 20f) { Trouble("enemy stuck in an attack", $"{enemy.name} in {state} for 20 s at {p:0.0}"); strikingFor[enemy] = 0f; }
        }
        if (lastAlive >= 0 && alive < lastAlive)
        {
            kills += lastAlive - alive;
            Say($"enemy down in {sceneName}: {alive} left, embers {RunState.Points}");
        }
        else if (lastAlive >= 0 && alive > lastAlive) Say($"enemies back in {sceneName}: {alive} alive");
        lastAlive = alive;

        if (RunState.Unlocked.Count != lastUnlocked)
        {
            if (RunState.Unlocked.Count > lastUnlocked && mode != "arena") { bought++; Say($"learned: now [{string.Join(",", RunState.Unlocked)}] embers {RunState.Points}"); }
            lastUnlocked = RunState.Unlocked.Count;
        }

        if (colossus != null)
        {
            string doing = colossus.Doing;
            if (doing != colossusDoing)
            {
                if (colossusDoing == "Hidden" || doing == "Dying" || doing == "Dead" || doing == "Staggered") Say($"colossus {colossusDoing} -> {doing} health {colossus.Health.Current:0}");
                colossusDoing = doing;
                colossusFor = 0f;
            }
            else colossusFor += Time.deltaTime;
            if (doing != "Idle" && doing != "Hidden" && doing != "Dead" && colossusFor > 25f) { Trouble("colossus stuck", $"in {doing} for 25 s"); colossusFor = 0f; }
        }
    }

    // Weakens whatever the kept-alive bot is fighting, so a bot that cannot aim still clears the run.
    private void Assist()
    {
        if (Time.time < nextAssist) return;
        nextAssist = Time.time + 0.4f;
        Vector3 at = player.transform.position;
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead) continue;
            Vector3 to = enemy.transform.position - at;
            if (to.sqrMagnitude < 25f) enemy.TakeHit(new Hit(30f, 10f, to.normalized, false, true));
        }
        if (colossus != null && !colossus.IsDead && colossus.Doing != "Hidden" && colossus.Doing != "Rising")
        {
            colossus.Unbalance(30f);
            colossus.HeadPart.TakeHit(new Hit(14f, 4f, Vector3.forward));
        }
    }

    private Component Objective()
    {
        Vector3 at = player.transform.position;
        Enemy nearest = null;
        float best = float.MaxValue;
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.IsDead) continue;
            float distance = (enemy.transform.position - at).sqrMagnitude;
            if (distance < best) { best = distance; nearest = enemy; }
        }
        if (sceneName == "Castle")
        {
            var stone = FindAnyObjectByType<WeaponPickup>();
            if (wantsKatana && stone != null && !stone.Taken) return stone;
            if (restPlanned && !restedThisLoop && RunState.Points >= 2)
            {
                RestPoint fire = null;
                float fireBest = float.MaxValue;
                foreach (var rest in FindObjectsByType<RestPoint>(FindObjectsSortMode.None))
                {
                    float d = (rest.transform.position - at).sqrMagnitude;
                    if (d < fireBest) { fireBest = d; fire = rest; }
                }
                if (fire != null) return fire;
            }
            if (nearest != null) return nearest;
            return FindAnyObjectByType<SealedWay>();
        }
        if (nearest != null) return nearest;
        if (sceneName == "Summit")
        {
            if (colossus != null && !colossus.IsDead) return null;
            if (rewardPick == null || !rewardPick.Available)
            {
                rewardPick = null;
                var rewards = FindObjectsByType<GodReward>(FindObjectsSortMode.None);
                var open = new List<GodReward>();
                foreach (var reward in rewards) if (reward.Available) open.Add(reward);
                if (open.Count > 0) rewardPick = open[random.Next(open.Count)];
            }
            if (rewardPick != null) return rewardPick;
            foreach (var door in FindObjectsByType<SceneDoor>(FindObjectsSortMode.None))
                if (door.Available && door.isActiveAndEnabled) return door;
            return null;
        }
        if (sceneName == "Hub")
        {
            if (wantsDetour)
            {
                var choices = FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None);
                if (choices.Length > 0)
                {
                    var choice = choices[(seed + runsHome) % choices.Length];
                    if ((choice.transform.position - at).sqrMagnitude > 4f) return choice;
                    wantsDetour = random.Next(100) < 40;
                }
            }
            return FindAnyObjectByType<SceneDoor>();
        }
        return null;
    }

    private static readonly Key[] Fight = { Key.J, Key.J, Key.J, Key.K, Key.K, Key.L, Key.Space, Key.Space, Key.R, Key.U, Key.I, Key.E };

    private void Play()
    {
        float now = Time.unscaledTime;
        if (releaseFrame)
        {
            // One frame with the pressed-once keys up, so the next press of the same key counts as a press.
            releaseFrame = false;
            held.RemoveAll(key => key != Key.W && key != Key.A && key != Key.S && key != Key.D && key != Key.L);
            if (MenuOpen || BoxOpen) held.Clear();
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held.ToArray()));
            return;
        }
        if (now < nextDecision) return;
        held.Clear();
        wantsMove = false;

        if (MenuOpen)
        {
            int roll = random.Next(100);
            held.Add(roll < 35 ? Key.E : roll < 60 ? Key.S : roll < 80 ? Key.W : roll < 92 ? Key.Space : Fight[random.Next(Fight.Length)]);
            nextDecision = now + 0.05f + (float)random.NextDouble() * 0.25f;
            if (!restedThisLoop) { restedThisLoop = true; Say($"menu open: embers {RunState.Points} enemies alive {(area != null ? area.EnemiesAlive : -1)}"); }
        }
        else if (BoxOpen)
        {
            held.Add(random.Next(100) < 75 ? Key.E : Fight[random.Next(Fight.Length)]);
            if (random.Next(4) == 0) held.Add(Key.W);
            nextDecision = now + 0.05f + (float)random.NextDouble() * 0.3f;
        }
        else
        {
            var target = Objective();
            Vector3 at = player.transform.position;
            Vector3 goal = target != null ? target.transform.position : spawn + new Vector3(Mathf.Sin(now * 0.37f), 0f, Mathf.Cos(now * 0.23f)) * 6f;
            Vector3 to = goal - at;
            to.y = 0f;
            float distance = to.magnitude;

            if (target != progressTarget) { progressTarget = target; progressBest = distance; progressSince = now; }
            if (distance < progressBest - 0.5f) { progressBest = distance; progressSince = now; }
            if (target != null && distance > 2.2f && now - progressSince > 3f && now > sidestepUntil)
            {
                // Walking straight at it is not working: go some other way for a moment, as a lost person would.
                float turn = (float)random.NextDouble() * Mathf.PI * 2f;
                sidestep = new Vector2(Mathf.Cos(turn), Mathf.Sin(turn));
                sidestepUntil = now + 0.6f + (float)random.NextDouble() * 1.2f;
            }
            if (mode == "clear" && target != null && distance > 2.2f && now - progressSince > 14f)
            {
                // The bot has no map. Carried there, so the soak goes on; counted, so it is not mistaken for play.
                assists++;
                player.Teleport(goal + new Vector3(0f, 0.15f, -1.6f), Quaternion.identity);
                progressSince = now;
                progressBest = 1.6f;
            }

            bool random_walk = random.Next(100) < (mode == "arena" ? 45 : 18);
            Vector2 stick;
            if (now < sidestepUntil) stick = sidestep;
            else if (random_walk || distance < 1.2f) stick = random.Next(5) == 0 ? Vector2.zero : new Vector2(random.Next(3) - 1, random.Next(3) - 1);
            else
            {
                // Which of the eight ways the keys can go lies nearest to where it wants to be.
                stick = Vector2.zero;
                float bestDot = -2f;
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        float dot = Vector3.Dot(player.CameraRelative(new Vector2(x, y)).normalized, to.normalized);
                        if (dot > bestDot) { bestDot = dot; stick = new Vector2(x, y); }
                    }
            }
            if (stick.x < 0f) held.Add(Key.A);
            if (stick.x > 0f) held.Add(Key.D);
            if (stick.y > 0f) held.Add(Key.W);
            if (stick.y < 0f) held.Add(Key.S);
            wantsMove = stick != Vector2.zero;

            bool fighting = target is Enemy && distance < 7f || colossus != null && !colossus.IsDead && colossus.Doing != "Hidden" || mode == "arena";
            float length = 0.1f + (float)random.NextDouble() * 0.3f;
            if (fighting || random.Next(100) < 25)
            {
                int count = random.Next(3);
                for (int i = 0; i < count; i++) held.Add(Fight[random.Next(Fight.Length)]);
                // Now and then a long hold: guard, the weapon's skill, a thrown flask, the spell's special.
                if (random.Next(100) < 14)
                {
                    Key[] holds = { Key.L, Key.K, Key.R, Key.U, Key.J, Key.I, Key.Space };
                    held.Add(holds[random.Next(holds.Length)]);
                    length = 0.5f + (float)random.NextDouble() * 1.4f;
                    wantsMove = false;
                }
            }
            bool wantsUse = interaction != null && interaction.Current != null && (target != null && !(target is Enemy) && interaction.Current == target || random.Next(100) < 12);
            if (wantsUse && !held.Contains(Key.E)) held.Add(Key.E);
            if (held.Contains(Key.L) || held.Contains(Key.R) || held.Contains(Key.Space)) wantsMove = false;

            if (target is SealedWay && area != null && area.EnemiesAlive == 0 && distance < 2.2f) doorRefusedFor += length; else doorRefusedFor = 0f;
            if (doorRefusedFor > 25f) { Trouble("sealed way refused", $"25 s at the sealed way with none alive, prompt target {(interaction.Current != null ? interaction.Current.name : "none")}"); doorRefusedFor = 0f; }
            nextDecision = now + length;
        }
        releaseFrame = true;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held.ToArray()));
    }

    private void Finish()
    {
        finished = true;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        var text = new StringBuilder();
        text.Append($"summary mode {mode} seed {seed}: game time {gameTime:0} s, real {Time.unscaledTime - startedAt:0} s, scenes {sceneChanges}, hub arrivals {runsHome}, deaths {deaths}, kills {kills}, learned {bought}, carried {assists}, spikes {spikes} (worst {worstSpike * 1000f:0} ms; {loadSpikes} more in the first frames of a scene), warnings {warnings}, error kinds {errors.Count}, trouble kinds {trouble.Count}");
        Say(text.ToString());
        foreach (var pair in trouble) Say($"trouble x{pair.Value}: {pair.Key}");
        foreach (var pair in errors) Say($"error x{pair.Value}: {pair.Key}");
        foreach (string sample in loops) Say("trend " + sample);
        Time.timeScale = 1f;
        ForgetAll();
        Say("done");
        Destroy(gameObject);
    }
}
