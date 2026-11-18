using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

// Play Mode probe for the controls of 2026-11-18, driven by AgentScripts/controls_probe.sh. Parts (the one argument):
//   "pad"    (Hub) a virtual Xbox pad reads the waking words, walks, strikes, opens the pause menu and walks it with the
//            d-pad and the stick, opens the controls page (frame controls_xbox), rebinds the flask to Y (a clash with the
//            spell, swapped; frame controls_rebind_pad), asks for the spell's key and waits for the script's T
//            (frame controls_rebound), then walks the chart and leaves it (frame chart_xbox).
//   "kept"   (any scene, a fresh play) says whether the rebinds above came back from PlayerPrefs.
//   "aim <x> <z>" says where on the screen the ground point (x, z) is, for simulate_pointer.
//   "dummy"  puts the player 3.5 m south-west of the nearest training dummy and names the point beside it to click.
//   "facing" says where the player faces, whether they attack, and the angle to the nearest dummy.
//   "keysmenu" opens the controls page as the keys left it (frame controls_keys) and says what it shows.
//   "click <name>" presses the left mouse button where simulate_pointer left the cursor, from the game's own frames
//            (the CLI's own presses reach held state but not WasPressedThisFrame with the editor in the background),
//            and says where the attack turned, in the frame it began (frame controls_<name>).
//   "keys"   walks the pause menu with keys pressed from the game's frames: Esc opens it, S chooses, E opens the
//            controls, Space goes back, Esc closes it.
//   "reset"  resets every binding through the page's bottom row (asked, then done) and says what is kept.
// Everything it writes goes to the sandbox key (the runner sets the sandbox flag); the owner's bindings are not touched.
public static class ControlsProbe
{
    public static string Run(string part)
    {
        if (!Application.isPlaying) return "not in play mode";
        string[] words = part.Split(' ');
        switch (words[0])
        {
            case "pad":
                new GameObject("Controls Probe").AddComponent<ControlsProbeRunner>();
                return "probe started";
            case "kept": return Kept();
            case "aim": return Aim(float.Parse(words[1]), float.Parse(words[2]));
            case "dummy": return Dummy();
            case "facing": return Facing();
            case "click":
                new GameObject("Controls Probe Click").AddComponent<ControlsProbeClick>().Shot_ = words.Length > 1 ? words[1] : "click";
                return "click started";
            case "keys":
                new GameObject("Controls Probe Menu Keys").AddComponent<ControlsProbeMenuKeys>();
                return "keys started";
            case "keysmenu":
                new GameObject("Controls Probe Keys").AddComponent<ControlsProbeKeys>();
                return "keys menu started";
            case "reset":
                new GameObject("Controls Probe Reset").AddComponent<ControlsProbeReset>();
                return "reset started";
        }
        return "unknown part " + part;
    }

    internal static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

    internal static string Binding(string action, bool pad, int nth = 0)
    {
        var found = Player.Controls.FindAction(action);
        int index = ControlBindings.IndexOf(found, pad ? ControlBindings.PadGroup : ControlBindings.KeysGroup, null, nth);
        return index < 0 ? "none" : found.bindings[index].effectivePath;
    }

    private static string Kept()
    {
        string key = ControlBindings.Key;
        return $"key {key} held {PlayerPrefs.HasKey(key)}; flask pad {Binding("Flask", true)}, spell pad {Binding("Spell", true)}, spell keys {Binding("Spell", false)}, "
            + $"real key held {PlayerPrefs.HasKey(ControlBindings.PrefsKey)}";
    }

    private static string Aim(float x, float z)
    {
        var player = Player;
        var at = new Vector3(x, player.transform.position.y, z);
        Vector3 screen = Camera.main.WorldToScreenPoint(at);
        return $"{screen.x:0} {screen.y:0}";
    }

    internal static TrainingDummy NearestDummy(Vector3 from)
    {
        TrainingDummy best = null;
        float bestDistance = float.MaxValue;
        foreach (var dummy in UnityEngine.Object.FindObjectsByType<TrainingDummy>())
        {
            float distance = (dummy.transform.position - from).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = dummy;
            }
        }
        return best;
    }

    private static string Dummy()
    {
        var player = Player;
        var dummy = NearestDummy(player.transform.position);
        if (dummy == null) return "no dummy";
        Vector3 spot = dummy.transform.position + new Vector3(-2.5f, 0f, -2.5f);
        spot.y = player.transform.position.y;
        var body = player.GetComponent<CharacterController>();
        body.enabled = false;
        player.transform.position = spot;
        // Facing away to the south-west, so a turn to the dummy is plain to see.
        player.transform.rotation = Quaternion.LookRotation(new Vector3(-1f, 0f, -1f));
        body.enabled = true;
        Vector3 d = dummy.transform.position;
        // A point on the ground 1.4 m to the east of the dummy: the cursor is near it, not on it.
        return $"dummy {d.x:0.00} {d.z:0.00} click {d.x + 1.4f:0.00} {d.z:0.00}";
    }

    internal static string Facing()
    {
        var player = Player;
        var combat = player.GetComponent<PlayerCombat>();
        Vector3 forward = player.transform.forward;
        var dummy = NearestDummy(player.transform.position);
        string toDummy = "";
        if (dummy != null)
        {
            Vector3 gap = dummy.transform.position - player.transform.position;
            gap.y = 0f;
            toDummy = $"; angle to the dummy {Vector3.Angle(forward, gap):0.0} deg at {gap.magnitude:0.00} m";
        }
        float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        return $"device {ControlDevices.Current} mouse aim {ControlDevices.MouseAim}; facing yaw {yaw:0.0} deg; attacking {combat.IsAttacking}{toDummy}";
    }
}

public abstract class ControlsProbeBase : MonoBehaviour
{
    protected static void Say(string text) => Debug.Log("[Probe] " + text);

    protected IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    protected static string Texts(Transform under)
    {
        if (under == null) return "";
        var line = new System.Text.StringBuilder();
        foreach (var text in under.GetComponentsInChildren<Text>(false))
            if (text.enabled && !string.IsNullOrEmpty(text.text)) line.Append(text.name).Append('=').Append(text.text.Replace("\n", " / ")).Append(" | ");
        return line.ToString();
    }

    protected static string PageText(int row)
    {
        var page = PauseMenu.Instance.ControlsPage;
        return $"{page.CellText(row, 0)} / {page.CellText(row, 1)} / {page.CellText(row, 2)}";
    }

    protected IEnumerator Until(Func<bool> condition, float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (!condition() && Time.realtimeSinceStartup < end) yield return null;
    }
}

public class ControlsProbeRunner : ControlsProbeBase
{
    private XInputControllerWindows pad;

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private IEnumerator Tap(ButtonControl button)
    {
        Set(button, 1f);
        yield return null;
        yield return null;
        Set(button, 0f);
        yield return null;
        yield return null;
    }

    private IEnumerator Stick(float x, float y, float seconds)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            pad.leftStick.x.WriteValueIntoEvent(x, eventPtr);
            pad.leftStick.y.WriteValueIntoEvent(y, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end) yield return null;
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            pad.leftStick.x.WriteValueIntoEvent(0f, eventPtr);
            pad.leftStick.y.WriteValueIntoEvent(0f, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        var player = ControlsProbe.Player;
        var combat = player.GetComponent<PlayerCombat>();
        var hud = PauseMenu.Instance.transform;
        Say($"pad added: {pad.layout}; bindings kept under {ControlBindings.Key} (sandboxed play)");

        // 1. The waking words, read on with A; the reading box names A.
        yield return Until(() => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen, 6f);
        int read = 0;
        string more = "";
        while (ReadingBox.Instance.IsOpen && read < 40)
        {
            yield return Tap(pad.buttonSouth);
            read++;
            var moreText = hud.Find("Reading Box/More")?.GetComponent<Text>();
            if (moreText != null && moreText.enabled) more = moreText.text;
        }
        Say($"1 words read with A: {read} presses, box open {ReadingBox.Instance.IsOpen}, its prompt says '{more}', device {ControlDevices.Current} {(more == "A" ? "OK" : "WRONG")}");
        var hint = hud.Find("Controls")?.GetComponent<Text>();
        yield return null;
        Say($"1 the HUD line: '{hint?.text}' {(hint != null && hint.text.StartsWith("Menu") ? "OK" : "WRONG")}");

        // 2. Walk and strike.
        yield return new WaitForSeconds(0.3f);
        Vector3 from = player.transform.position;
        yield return Stick(0f, 1f, 0.7f);
        float moved = (player.transform.position - from).magnitude;
        Say($"2 stick up 0.7 s: moved {moved:0.00} m {(moved > 2f ? "OK" : "WRONG")}");
        from = player.transform.position;
        Set(pad.dpad.right, 1f);
        yield return new WaitForSeconds(0.5f);
        Set(pad.dpad.right, 0f);
        yield return null;
        moved = (player.transform.position - from).magnitude;
        Say($"2 d-pad right 0.5 s: moved {moved:0.00} m {(moved > 1f ? "OK" : "WRONG")}");
        Set(pad.rightShoulder, 1f);
        yield return null;
        yield return null;
        bool attacking = combat.IsAttacking;
        Set(pad.rightShoulder, 0f);
        Say($"2 RB: attacking {attacking} {(attacking ? "OK" : "WRONG")}");
        yield return new WaitForSeconds(1.2f);

        // 3. The pause menu: Menu opens it, the d-pad and the stick walk it, A picks, B goes back.
        yield return Tap(pad.startButton);
        var menu = PauseMenu.Instance;
        var hintLine = hud.Find("Pause Menu/Panel/Hint")?.GetComponent<Text>();
        Say($"3 Menu: open {menu.IsOpen}; hint '{hintLine?.text}' {(menu.IsOpen && hintLine != null && hintLine.text.Contains("A: pick") && hintLine.text.Contains("Menu: resume") ? "OK" : "WRONG")}");
        yield return Tap(pad.dpad.down);
        int afterDpad = menu.Choices.Selected;
        yield return Stick(0f, -1f, 0.05f);
        int afterStick = menu.Choices.Selected;
        yield return Stick(0f, 1f, 0.05f);
        yield return Tap(pad.dpad.up);
        Say($"3 d-pad down: 0 -> {afterDpad}; stick down: -> {afterStick}; back up to {menu.Choices.Selected} {(afterDpad == 1 && afterStick == 2 && menu.Choices.Selected == 0 ? "OK" : "WRONG")}");
        yield return Tap(pad.dpad.down);
        yield return Tap(pad.buttonSouth);
        var page = menu.ControlsPage;
        Say($"3 A on '{PauseChoices.Label(menu.Choices.Current)}': controls shown {menu.ShowingControls} {(menu.ShowingControls ? "OK" : "WRONG")}");
        yield return Shot("controls_xbox");
        string light = PageText(4), flask = PageText(8), pause = PageText(12), move = PageText(0);
        Say($"3 the page in Xbox names: move {move}; light {light}; flask {flask}; pause {pause}; hint '{hintLine?.text}' "
            + $"{(light.StartsWith("RB / Left mouse / J") && flask.StartsWith("X / R") && pause.StartsWith("Menu / Esc") ? "OK" : "WRONG")}");
        Say($"3 page texts: {Texts(hud.Find("Pause Menu/Panel/Controls"))}");

        // 4. A pad rebind with the pad: the flask to Y, which the spell had; they swap.
        while (page.SelectedRow < 8) yield return Tap(pad.dpad.down);
        while (page.Column > 0) yield return Tap(pad.dpad.left);
        Say($"4 chosen: row {page.SelectedRow} column {page.Column}");
        yield return Tap(pad.buttonSouth);
        yield return Until(() => page.Listening, 2f);
        Say($"4 listening {page.Listening}; hint '{hintLine?.text}'");
        yield return Shot("controls_rebind_pad");
        yield return Tap(pad.buttonNorth);
        yield return Until(() => !page.Busy, 3f);
        Say($"4 after Y: '{page.Message}'; flask pad {ControlsProbe.Binding("Flask", true)}, spell pad {ControlsProbe.Binding("Spell", true)}, kept {PlayerPrefs.HasKey(ControlBindings.Key)} "
            + $"{(ControlsProbe.Binding("Flask", true) == "<Gamepad>/buttonNorth" && ControlsProbe.Binding("Spell", true) == "<Gamepad>/buttonWest" ? "OK" : "WRONG")}");

        // 5. A keys rebind asked for with the pad; the key itself comes from the script's simulate_key T.
        yield return Tap(pad.dpad.down);
        yield return Tap(pad.dpad.right);
        Say($"5 chosen: row {page.SelectedRow} column {page.Column}");
        yield return Tap(pad.buttonSouth);
        yield return Until(() => page.Listening, 2f);
        yield return Shot("controls_rebind_listening");
        Say($"5 listening keys {page.Listening}");
        yield return Until(() => !page.Busy, 20f);
        Say($"5 after the key: '{page.Message}'; spell keys {ControlsProbe.Binding("Spell", false)} {(ControlsProbe.Binding("Spell", false) == "<Keyboard>/t" ? "OK" : "WRONG")}");
        yield return Tap(pad.dpad.down);
        yield return Shot("controls_rebound");
        Say($"5 the page now: spell {PageText(9)}; flask {PageText(8)}");
        yield return Tap(pad.buttonEast);
        Say($"5 B: controls shown {menu.ShowingControls}, menu open {menu.IsOpen}");
        yield return Tap(pad.startButton);
        Say($"5 Menu: menu open {menu.IsOpen} {(!menu.IsOpen ? "OK" : "WRONG")}");
        yield return new WaitForSeconds(0.2f);

        // 6. The chart: walked with the d-pad, RT for the whole ring, B to leave.
        var chart = UpgradeMenu.Instance;
        chart.Open();
        yield return null;
        yield return null;
        int first = chart.Selected;
        Set(pad.dpad.up, 1f);
        yield return new WaitForSecondsRealtime(0.1f);
        Set(pad.dpad.up, 0f);
        yield return null;
        yield return null;
        int second = chart.Selected;
        var chartHint = hud.Find("Upgrade Menu/Hint")?.GetComponent<Text>();
        yield return Shot("chart_xbox");
        yield return Tap(pad.rightTrigger);
        bool wide = chart.IsWide;
        yield return Tap(pad.rightTrigger);
        yield return Tap(pad.buttonEast);
        Say($"6 chart: open, d-pad up moved {first} -> {second}; RT wide {wide}; hint '{chartHint?.text}'; B closed {!chart.IsOpen} "
            + $"{(wide && !chart.IsOpen && chartHint != null && chartHint.text.Contains("A: learn") ? "OK" : "WRONG")}");

        InputSystem.RemoveDevice(pad);
        Say("done");
        Destroy(gameObject);
    }
}

// The controls page after keys and mouse were the last used: opened by the page itself (no pad), pictured, read.
public class ControlsProbeKeys : ControlsProbeBase
{
    private IEnumerator Start()
    {
        var menu = PauseMenu.Instance;
        if (!menu.IsOpen) menu.Open();
        yield return null;
        if (!menu.ShowingControls)
        {
            while (menu.Choices.Current != PauseEntry.Controls) menu.Move(1);
            menu.Pick();
        }
        yield return null;
        yield return null;
        var hint = menu.transform.Find("Pause Menu/Panel/Hint")?.GetComponent<Text>();
        yield return Shot("controls_keys");
        Say($"keys page: device {ControlDevices.Current}; light {PageText(4)}; spell {PageText(9)}; flask {PageText(8)}; hint '{hint?.text}'");
        Say("keysmenu done");
        Destroy(gameObject);
    }
}

// Resets through the page's own bottom row: the first press asks, the second does it.
public class ControlsProbeReset : ControlsProbeBase
{
    private IEnumerator Start()
    {
        var menu = PauseMenu.Instance;
        if (!menu.IsOpen) menu.Open();
        yield return null;
        if (!menu.ShowingControls)
        {
            while (menu.Choices.Current != PauseEntry.Controls) menu.Move(1);
            menu.Pick();
        }
        yield return null;
        var page = menu.ControlsPage;
        page.Choose(ControlsPage.ResetRow, 0);
        page.Pick();
        yield return null;
        bool asked = page.AskingReset;
        yield return Shot("controls_reset_ask");
        page.Pick();
        yield return null;
        yield return Shot("controls_reset_done");
        Say($"reset: asked first {asked}; '{page.Message}'; kept {PlayerPrefs.HasKey(ControlBindings.Key)}; flask pad {ControlsProbe.Binding("Flask", true)}, spell keys {ControlsProbe.Binding("Spell", false)} "
            + $"{(asked && !PlayerPrefs.HasKey(ControlBindings.Key) && ControlsProbe.Binding("Flask", true) == "<Gamepad>/buttonWest" && ControlsProbe.Binding("Spell", false) == "<Keyboard>/q" ? "OK" : "WRONG")}");
        menu.Close();
        Say("reset done");
        Destroy(gameObject);
    }
}

// A left click where the cursor is, pressed and let go from the game's frames; the attack's turn is read as it begins.
public class ControlsProbeClick : ControlsProbeBase
{
    public string Shot_ = "click";

    private IEnumerator Start()
    {
        var mouse = Mouse.current;
        var player = ControlsProbe.Player;
        var combat = player.GetComponent<PlayerCombat>();
        Vector2 at = mouse.position.ReadValue();
        bool pointed = player.PointerAim(out Vector3 toward);
        float before = Mathf.Atan2(player.transform.forward.x, player.transform.forward.z) * Mathf.Rad2Deg;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = at }.WithButton(MouseButton.Left, true));
        yield return Until(() => combat.IsAttacking, 1f);
        string facing = ControlsProbe.Facing();
        yield return Shot("controls_" + Shot_);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = at }.WithButton(MouseButton.Left, false));
        Say($"click {Shot_} at {at}: cursor direction {(pointed ? (Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg).ToString("0") : "none")} deg, facing before {before:0} deg; at the attack: {facing}");
        Say("click done");
        Destroy(gameObject);
    }
}

// The pause menu walked with keys pressed from the game's frames.
public class ControlsProbeMenuKeys : ControlsProbeBase
{
    private IEnumerator Tap(Key key)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(key));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        var menu = PauseMenu.Instance;
        yield return Tap(Key.Escape);
        bool opened = menu.IsOpen;
        var hint = menu.transform.Find("Pause Menu/Panel/Hint")?.GetComponent<Text>();
        string hintText = hint?.text;
        yield return Tap(Key.S);
        int chosen = menu.Choices.Selected;
        yield return Tap(Key.E);
        bool controls = menu.ShowingControls;
        yield return Shot("controls_keys_walked");
        yield return Tap(Key.Space);
        bool back = !menu.ShowingControls && menu.IsOpen;
        yield return Tap(Key.Escape);
        Say($"keys: Esc opened {opened}; hint '{hintText}'; S chose {chosen}; E controls {controls}; Space back {back}; Esc closed {!menu.IsOpen} "
            + $"{(opened && chosen == 1 && controls && back && !menu.IsOpen && hintText != null && hintText.Contains("E: pick") ? "OK" : "WRONG")}");
        Say("keys done");
        Destroy(gameObject);
    }
}
