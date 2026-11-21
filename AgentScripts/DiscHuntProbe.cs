using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

// Finds what draws the pale-white disc over the enemy when the Katana's Fire infused strike lands (the swing films of
// 2026-11-20, frames 53 to 60). A castle soldier is let out in the Proving Ground, the player stands 1.6 m off with the
// Katana and Fire, and the infused strike is pressed. Every renderer that is drawn within 4 m of the soldier 0.05 to
// 0.45 s after the press, and was not drawn just before it, is listed with its material, shader and colour, and every
// particle system there with live particles and their start colour. Frame review/shots/current/dischunt.png.
// bash AgentScripts/runprobe.sh ProvingGround DiscHuntProbe 40
public static class DiscHuntProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Disc Hunt Probe").AddComponent<DiscHuntProbeRunner>();
        return "probe started";
    }
}

public class DiscHuntProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerCombat combat;
    private PlayerBuild build;
    private Enemy soldier;
    private EnemyPen castle;
    private XInputController pad;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (soldier != null && !soldier.IsDead)
        {
            if (soldier.Health.Fraction < 0.6f) soldier.Health.Fill();
            soldier.GuardOpenFor = 5f;
            soldier.Brain.AggroRange = 0f;
        }
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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private IEnumerator Tap(Key key)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(key));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        yield return null;
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        if (GuidePage.Showing) yield return Tap(Key.Space);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (Time.timeScale <= 0f) Say("WRONG: time still stopped after the guide");
    }

    private HashSet<Renderer> Drawn(Vector3 near)
    {
        var set = new HashSet<Renderer>();
        foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
            if (renderer.enabled && renderer.isVisible && (renderer.bounds.center - near).sqrMagnitude < 16f) set.Add(renderer);
        return set;
    }

    private static string Path(Transform t)
    {
        string path = t.name;
        for (int i = 0; i < 3 && t.parent != null; i++) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    private static string Colour(Material material)
    {
        if (material == null) return "no material";
        foreach (string id in new[] { "_Color", "_BaseColor", "_TintColor", "_EmissionColor" })
            if (material.HasProperty(id)) return $"{id} {material.GetColor(id)}";
        return "no colour";
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        playtestWas = PlaytestMode.On;
        PlaytestMode.Set(true);
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        combat = player.GetComponent<PlayerCombat>();
        build = player.GetComponent<PlayerBuild>();
        Say($"sandboxed {Progress.Sandboxed}");
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).FirstOrDefault(pen => pen.Land == "The castle");
        castle.Release(new[] { 0 });
        soldier = castle.Enemies[0];
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        build.Attune(Elements.Fire, "");
        yield return Real(0.8f);
        Vector3 at = soldier.transform.position + Vector3.back * 1.6f;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.4f);

        // Nearly broken, so the strike staggers it and the finish mark lands, as in the film.
        soldier.Stagger.Add(soldier.Stagger.Maximum * 0.95f - soldier.Stagger.Value);
        Vector3 centre = soldier.transform.position + Vector3.up * 1.2f;
        var before = Drawn(centre);
        int strikes = combat.InfusedStrikes;
        Set(pad.leftTrigger, 1f);
        yield return null;
        yield return null;
        Set(pad.rightShoulder, 1f);
        yield return null;
        yield return null;
        Set(pad.rightShoulder, 0f);
        yield return null;
        Set(pad.leftTrigger, 0f);
        float start = Time.unscaledTime;
        var seen = new Dictionary<Renderer, float>();
        // Six frames from the moment the strike lands, a tenth of a second apart: the mark landing and the stagger ring.
        int shots = 0;
        float landedAt = -1f;
        while (Time.unscaledTime - start < 1.2f)
        {
            centre = soldier.transform.position + Vector3.up * 1.2f;
            foreach (var renderer in Drawn(centre))
                if (!before.Contains(renderer) && !seen.ContainsKey(renderer)) seen[renderer] = Time.unscaledTime - start;
            if (landedAt < 0f && soldier.IsStaggered) landedAt = Time.unscaledTime;
            if (landedAt >= 0f && shots < 6 && Time.unscaledTime - landedAt >= shots * 0.1f)
            {
                ScreenCapture.CaptureScreenshot($"review/shots/current/dischunt_{shots}.png");
                shots++;
            }
            yield return null;
        }
        Say($"frames saved {shots}, landed at +{(landedAt >= 0f ? landedAt - start : -1f):0.00}s; blows landed {combat.BlowsLanded}, player {player.transform.position}, soldier {soldier.transform.position}, weapon {combat.Weapon?.Name}, time scale {Time.timeScale}");
        Say($"infused strikes {combat.InfusedStrikes - strikes}, staggered {soldier.IsStaggered}, new renderers near the soldier {seen.Count}");
        foreach (var pair in seen.OrderBy(pair => pair.Value))
        {
            var renderer = pair.Key;
            if (renderer == null) continue;
            var material = renderer.sharedMaterial;
            string extra = "";
            if (renderer is ParticleSystemRenderer)
            {
                var system = renderer.GetComponent<ParticleSystem>();
                extra = $" particles {system.particleCount}, start colour {system.main.startColor.color}";
            }
            Say($"+{pair.Value:0.00}s {renderer.GetType().Name} '{Path(renderer.transform)}' material '{(material != null ? material.name : "none")}' shader '{(material != null ? material.shader.name : "-")}' {Colour(material)} size {renderer.bounds.size.x:0.0}x{renderer.bounds.size.y:0.0}x{renderer.bounds.size.z:0.0}{extra}");
        }
        if (castle != null) castle.Clear();
        combat.Equip(Weapons.ByName(Weapons.FistsName));
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }
}
