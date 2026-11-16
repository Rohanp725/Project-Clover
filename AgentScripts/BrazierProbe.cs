using System.Collections;
using Clover.Core;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;

// Play Mode probe for the hub's eleven braziers: counts them, says which are lit, carries an element from one of the
// nine new ones, and photographs the row. Run in the hub:  bash AgentScripts/runprobe.sh Hub BrazierProbe
public static class BrazierProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Brazier Probe").AddComponent<BrazierProbeRunner>();
        return "probe started";
    }
}

public class BrazierProbeRunner : MonoBehaviour
{
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var element = player.GetComponent<PlayerElement>();
        var interaction = player.GetComponent<PlayerInteraction>();
        // The sponsor speaks first on a first waking; its words are put away so the island can be seen.
        yield return new WaitForSecondsRealtime(1f);
        for (int i = 0; i < 12 && ReadingBox.Instance != null && ReadingBox.Instance.IsOpen; i++)
        {
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.E));
            yield return null;
            yield return null;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState());
            yield return new WaitForSecondsRealtime(0.7f);
        }
        string lit = "", cold = "";
        int braziers = 0;
        LoadoutChoice ice = null;
        foreach (var choice in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
        {
            if (!choice.IsElement) continue;
            braziers++;
            if (choice.IsUnlocked) lit += choice.Choice + " ";
            else cold += choice.Choice + " ";
            if (choice.Choice == Elements.Ice) ice = choice;
        }
        player.Teleport(new Vector3(3.4f, 0.05f, -1.5f), Quaternion.Euler(0f, 80f, 0f));
        yield return new WaitForSeconds(0.8f);
        ScreenCapture.CaptureScreenshot("review/shots/current/hub_braziers.png");
        yield return null;
        yield return null;
        player.Teleport(ice.transform.position + (Vector3.zero - ice.transform.position).normalized * 1.6f, Quaternion.LookRotation(ice.transform.position - player.transform.position));
        yield return new WaitForSeconds(0.5f);
        string prompt = interaction.Current != null ? interaction.Current.Prompt : "nothing";
        ice.Use(interaction);
        yield return new WaitForSeconds(0.6f);
        ScreenCapture.CaptureScreenshot("review/shots/current/hub_brazier_ice.png");
        yield return null;
        yield return null;
        Say($"braziers {braziers}; lit: {lit}; cold: {cold}; standing by Ice the prompt is '{prompt}'; taken: element {element.Kit.Name} spell {element.Kit.SpellName} run element '{RunState.Element}'");
        Say("done");
        Destroy(gameObject);
    }
}
