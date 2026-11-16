using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for the hub and the run's loop: the sponsor's first words, the stands and braziers sealed and then
// open, the gate to the castle, a fall that ends the run, and waking in the hub again. It forgets all saved progress
// before and after, so the player's own first waking is untouched. Run in Play Mode, in the Hub scene, with
//   unity command script run -- --file AgentScripts/HubProbe.cs --entry HubProbe.Run
public static class HubProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Hub Probe").AddComponent<HubProbeRunner>();
        return "probe started";
    }
}

public class HubProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerInteraction interaction;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static void Shot(string name) => ScreenCapture.CaptureScreenshot("review/shots/current/hub_" + name + ".png");

    private static void ForgetAll()
    {
        Progress.Forget(Progress.MetSponsor);
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
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

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        interaction = player.GetComponent<PlayerInteraction>();
    }

    // Reads whatever the box is showing to its end, and returns how many presses it took.
    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 60) yield return Tap(Key.E);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        SceneManager.LoadScene("Hub");
        yield return null;
        yield return null;
        FindPlayer();
        Say($"start: scene {SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} element {element.Kit.Name} runs ended {RunState.RunsEnded}");
        Shot("arrive");

        // A: the first waking. The sponsor speaks unasked.
        yield return Until(() => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen, 4f);
        bool spoke = ReadingBox.Instance != null && ReadingBox.Instance.IsOpen;
        bool held = player.Suspended;
        yield return new WaitForSeconds(0.8f);
        Shot("voice");
        yield return null;
        int before = ReadingBox.Instance.LinesRead;
        yield return ReadAll();
        yield return null;
        yield return null;
        Say($"A first waking: spoke unasked {spoke} player held {held} lines read {ReadingBox.Instance.LinesRead - before} of {SponsorLines.FirstWaking.Length} closed {!ReadingBox.Instance.IsOpen} player free {!player.Suspended} met {Progress.Has(Progress.MetSponsor)}");

        // B: the Katana's stand is empty until the sword has been drawn in a run.
        LoadoutChoice katana = null, storm = null, fire = null, fists = null;
        foreach (var choice in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
        {
            if (choice.Choice == Weapons.KatanaName) katana = choice;
            if (choice.Choice == ElementKit.LightningName) storm = choice;
            if (choice.Choice == ElementKit.FireName) fire = choice;
            if (choice.Choice == Weapons.FistsName) fists = choice;
        }
        player.Teleport(katana.transform.position + new Vector3(1.3f, 0.05f, 0.6f), Quaternion.LookRotation(Vector3.left));
        yield return new WaitForSeconds(0.4f);
        string prompt = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return Tap(Key.E);
        Say($"B sealed stand: prompt '{prompt}' says '{defence.LastCallout}' weapon {combat.Weapon.Name}");
        Progress.Unlock(Progress.WeaponKey(Weapons.KatanaName));
        yield return new WaitForSeconds(0.5f);
        prompt = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Shot("katana");
        Say($"B open stand: prompt '{prompt}' weapon {combat.Weapon.Name} run weapon '{RunState.Weapon}'");

        // C: the same for Lightning's brazier.
        player.Teleport(storm.transform.position + new Vector3(-1.3f, 0.05f, 0.6f), Quaternion.LookRotation(Vector3.right));
        yield return new WaitForSeconds(0.4f);
        prompt = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return Tap(Key.E);
        string sealedWords = defence.LastCallout;
        Progress.Unlock(Progress.ElementKey(ElementKit.LightningName));
        yield return new WaitForSeconds(0.5f);
        string open = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Shot("lightning");
        Say($"C brazier: sealed prompt '{prompt}' says '{sealedWords}'; open prompt '{open}' element {element.Kit.Name} run element '{RunState.Element}'");

        // D: the whole place, from where the player wakes.
        player.Teleport(new Vector3(0f, 0.05f, -4.6f), Quaternion.identity);
        yield return new WaitForSeconds(0.6f);
        Shot("whole");
        yield return null;
        player.Teleport(new Vector3(0f, 0.05f, 5f), Quaternion.identity);
        yield return new WaitForSeconds(0.6f);
        Shot("gate");
        yield return null;

        // E: through the gate, carrying what was chosen.
        var gate = FindAnyObjectByType<SceneDoor>();
        player.Teleport(gate.transform.position + new Vector3(0f, 0.05f, -1.8f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        prompt = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return Tap(Key.E);
        yield return Until(() => SceneManager.GetActiveScene().name == "Castle", 5f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"E gate: prompt '{prompt}' scene {SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} element {element.Kit.Name} embers {RunState.Points}");

        // F: a fall ends the run and the player wakes in the hub with nothing.
        RunState.Points = 3;
        yield return new WaitForSeconds(0.5f);
        defence.TakeHit(new Hit(1000f, 0f, Vector3.back));
        yield return null;
        var ender = FindAnyObjectByType<RunEnder>();
        bool dead = defence.IsDead;
        yield return new WaitForSecondsRealtime(2.2f);
        Shot("fall");
        yield return Until(() => SceneManager.GetActiveScene().name == "Hub", 8f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"F fall: dead {dead} run ender {(ender != null)} scene {SceneManager.GetActiveScene().name} runs ended {RunState.RunsEnded} fell {RunState.LastRunFell} weapon {combat.Weapon.Name} element {element.Kit.Name} embers {RunState.Points}");

        // G: spoken to, the sponsor now has words about the fall, and then nothing more.
        var sponsor = FindAnyObjectByType<Sponsor>();
        player.Teleport(sponsor.transform.position + new Vector3(0f, 0.05f, -2f), Quaternion.identity);
        yield return new WaitForSeconds(1.8f);
        bool unasked = ReadingBox.Instance.IsOpen;
        yield return Tap(Key.E);
        yield return Until(() => ReadingBox.Instance.IsOpen, 1f);
        yield return new WaitForSeconds(0.7f);
        Shot("after_fall");
        yield return null;
        before = ReadingBox.Instance.LinesRead;
        yield return ReadAll();
        int first = ReadingBox.Instance.LinesRead - before;
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.E);
        yield return Until(() => ReadingBox.Instance.IsOpen, 1f);
        before = ReadingBox.Instance.LinesRead;
        yield return ReadAll();
        Say($"G after a fall: spoke unasked {unasked}; asked, lines {first}; asked again, lines {ReadingBox.Instance.LinesRead - before}; the stand still open {FindStand(Weapons.KatanaName).IsUnlocked}");

        ForgetAll();
        Say("done");
        Destroy(gameObject);
    }

    private static LoadoutChoice FindStand(string choice)
    {
        foreach (var stand in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
            if (stand.Choice == choice) return stand;
        return null;
    }
}
