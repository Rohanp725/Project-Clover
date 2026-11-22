using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.UI;
using Clover.UI.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

// The Loadout's editor-step checks (ui_rest_fire_plan.md 7, steps 7, 9 and 10), stills only, in the Proving Ground.
// Parts from AgentScripts/loadout_walk.txt (comma list): rich (the Katana with the Sheath, one keystone bought and not
// set, the other locked, a saved Hand), layout (rects of the board's blocks to the log), stills (the plan's states at
// 100, 125 and 150%), pad (every place, a list, Y, held X, LB/RB, L3, B from every depth, by simulated pad presses),
// mouse (moves and clicks by simulated mouse events). Stills review/shots/current/loadout/<w>x<h>_<state>.png.
//   bash AgentScripts/runprobe.sh ProvingGround LoadoutWalk 120
public static class LoadoutWalk
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Loadout Walk").AddComponent<LoadoutWalkRunner>();
        return "probe started";
    }
}

public class LoadoutWalkRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/loadout";
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private int fails;
    private void Judge(string what, bool ok, string detail = "")
    {
        if (!ok) fails++;
        Say($"{(ok ? "OK  " : "FAIL")} {what}{(detail.Length > 0 ? " (" + detail + ")" : "")}");
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

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

    private Gamepad pad;
    private IEnumerator Pad(GamepadButton button, float hold = 0f)
    {
        InputSystem.QueueStateEvent(pad, new GamepadState(button));
        yield return null;
        yield return null;
        if (hold > 0f) yield return Real(hold);
        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return null;
        yield return null;
    }

    private IEnumerator Stick(Vector2 lean)
    {
        InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = lean });
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return null;
        yield return null;
    }

    private IEnumerator Still(string name)
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot($"{Folder}/{Screen.width}x{Screen.height}_{name}.png");
        yield return null;
        yield return null;
    }

    private LoadoutScreen screen;
    private LoadoutController C => screen.Controller;
    private VisualElement Root => screen.GetComponent<UIDocument>().rootVisualElement;

    private IEnumerator Start()
    {
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        string parts = File.Exists("AgentScripts/loadout_walk.txt") ? File.ReadAllText("AgentScripts/loadout_walk.txt").Trim() : "layout,stills";
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        float before = Time.time;
        yield return Real(0.5f);
        Judge("guide closed and time runs", !GuidePage.Showing && Time.time > before, $"time {before:0.00} -> {Time.time:0.00}");
        // A pad of the probe's own: a real pad plugged in (a DualShock's HID report) does not take GamepadState events.
        pad = InputSystem.AddDevice<Gamepad>("Probe Pad");
        yield return null;

        if (parts.Contains("rich")) yield return Rich();
        screen = LoadoutScreen.Ensure();
        bool opened = screen != null && screen.Open();
        yield return Real(0.6f);
        Judge("the Loadout opens", opened && LoadoutScreen.Showing, $"pictures {(Glyphs.Pictures != null ? Glyphs.Pictures.Count : -1)}");
        if (opened)
        {
            if (parts.Contains("layout")) Layout();
            if (parts.Contains("stills")) yield return Stills();
            if (parts.Contains("pad")) yield return PadWalk();
            if (parts.Contains("mouse")) yield return MouseWalk();
            if (parts.Contains("hold"))
            {
                Say("held open for live checks");
                yield break;
            }
            if (LoadoutScreen.Showing) screen.Close();
        }
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        // The walk's L3 saved a text size; the owner's own setting (none) comes back.
        PlayerPrefs.DeleteKey(TextSizeSetting.PrefsKey);
        PlayerPrefs.Save();
        Say($"done: {fails} failed");
        UnityEditor.EditorApplication.ExitPlaymode();
    }

    // ---- The Katana with the Sheath, The Cut Lands at the Click bought and not set, A Hundred Days locked, a saved Hand.
    private IEnumerator Rich()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var combat = player.GetComponent<PlayerCombat>();
        var upgrades = player.GetComponent<PlayerUpgrades>();
        var build = player.GetComponent<PlayerBuild>();
        if (combat.Weapon == null || combat.Weapon.Name != Weapons.KatanaName) combat.Arm(Weapons.KatanaName);
        yield return null;
        foreach (string id in new[] { "katana.sheath", "katana.sheath.sayabiki", "katana.sheath.click" }) Learn(upgrades, id, 0);
        WayRules.ChooseAspect(RunState.WayChoices, Upgrade.ById("katana.sheath"), RunState.Unlocked, PlaytestMode.On);
        combat.Rearm();
        upgrades.RefreshNow();
        RunState.Points = 340;
        build.SaveHand(0);
        yield return null;
        Say($"rich: weapon {combat.Weapon?.Name}, learned {RunState.Unlocked.Count}, aspect {RunState.WayChoices}");
    }

    private void Learn(PlayerUpgrades upgrades, string id, int depth)
    {
        var mark = Upgrade.ById(id);
        if (mark == null || depth > 12 || RunState.Unlocked.Contains(id)) return;
        if (!upgrades.CanBuy(mark) && mark.Requires != null && mark.Requires.Length > 0 && !mark.Requires.Any(RunState.Unlocked.Contains)) Learn(upgrades, mark.Requires[0], depth + 1);
        var wallet = Progress.Data.Wallet;
        if (mark.Currency == Currency.Memories && wallet.Memories < mark.Cost) wallet.Earn(Currency.Memories, mark.Cost - wallet.Memories);
        bool bought = upgrades.TryBuy(mark);
        upgrades.RefreshNow();
        if (!bought) Say($"  (could not buy {mark.Name})");
    }

    // ---- Rects of the board's blocks, in board pixels, and every place that overlaps another.
    private void Layout()
    {
        var board = Root.Q("board");
        var origin = board.worldBound.position;
        var sb = new StringBuilder("layout:");
        var rects = new List<(string, Rect)>();
        for (int i = 0; i < LoadoutPlaces.Names.Length; i++)
        {
            var place = Root.Q(LoadoutPlaces.Names[i]);
            if (place == null || !Seen(place)) continue;
            var r = place.worldBound;
            r.position -= origin;
            rects.Add((LoadoutPlaces.Names[i], r));
            sb.Append($" {LoadoutPlaces.Names[i]}[{r.x:0},{r.y:0},{r.width:0}x{r.height:0}]");
        }
        Say(sb.ToString());
        for (int a = 0; a < rects.Count; a++)
            for (int b = a + 1; b < rects.Count; b++)
                if (rects[a].Item2.Overlaps(rects[b].Item2))
                    Judge($"{rects[a].Item1} clear of {rects[b].Item1}", false, $"{rects[a].Item2} vs {rects[b].Item2}");
        foreach (string name in new[] { "bearing-1", "keystone-1", "element-header", "aspect" })
        {
            var detail = new StringBuilder($"inside {name}:");
            Root.Q(name)?.Query<VisualElement>().ForEach(part =>
            {
                if (part.resolvedStyle.display == DisplayStyle.None) return;
                var r = part.worldBound;
                string what = part is Label l ? "'" + l.text + "'" : part.GetType().Name + "." + string.Join(".", part.GetClasses());
                detail.Append($" {what}[{r.x - origin.x:0},{r.y - origin.y:0},{r.width:0}x{r.height:0}]");
            });
            Say(detail.ToString());
        }
        var bottom = Root.Q("element-carried");
        if (bottom != null) Say($"element-carried bottom {bottom.worldBound.yMax - origin.y:0}, west top {Root.Q("slot-west").worldBound.y - origin.y:0}");
        // Words that run past their card.
        Root.Query<Label>().ForEach(label =>
        {
            if (!Seen(label) || string.IsNullOrEmpty(label.text)) return;
            var card = label.parent; while (card != null && !card.ClassListContains("place")) card = card.parent;
            // The surge's name sits beside its ring by design.
            if (card == null || card.name == "surge") return;
            var outer = card.worldBound;
            var r = label.worldBound;
            if (r.xMax > outer.xMax + 1 || r.yMax > outer.yMax + 1) Judge($"'{label.text}' inside {card.name}", false, $"label {r.xMax - origin.x:0},{r.yMax - origin.y:0} card {outer.xMax - origin.x:0},{outer.yMax - origin.y:0}");
        });
    }

    // Drawn: neither it nor any part above it hidden, and not see-through.
    private static bool Seen(VisualElement element)
    {
        if (element.worldBound.width < 1f || element.worldBound.height < 1f) return false;
        for (var at = element; at != null; at = at.parent)
            if (at.resolvedStyle.display == DisplayStyle.None || at.resolvedStyle.opacity < 0.01f) return false;
        return true;
    }

    // ---- The plan's states (7, step 9).
    private IEnumerator Stills()
    {
        foreach (int size in new[] { 100, 125, 150 })
        {
            C.SetTextSize(size);
            C.PointAt(LoadoutPlace.Starter);
            yield return Real(0.2f);
            string s = "_" + size;
            yield return Still("opened" + s);
            if (size == 100) Layout();
            C.PointAt(LoadoutPlace.Keystone1);
            yield return Still("keystone_unset" + s);
            C.PointAt(LoadoutPlace.Keystone2);
            yield return Still("keystone_locked" + s);
            C.PointAt(LoadoutPlace.Hand1);
            yield return Still("saved_hand" + s);
            C.PointAt(LoadoutPlace.South);
            C.Submit();
            yield return Real(0.2f);
            Judge($"a spell list opens at {size}%", C.Mode == LoadoutMode.SpellList, C.Mode.ToString());
            yield return Still("list" + s);
            C.Cancel();
            C.PointAt(LoadoutPlace.Aspect);
            C.Submit();
            yield return Real(0.2f);
            Judge($"the aspect cards open at {size}%", C.Mode == LoadoutMode.AspectCards, C.Mode.ToString());
            yield return Still("cards" + s);
            C.Cancel();
            if (size > 100)
            {
                C.PointAt(LoadoutPlace.Aspect);
                yield return Real(0.2f);
                Judge($"the Element half folds with Steel in hand at {size}%", C.Folded == LoadoutHalf.Element, C.Folded.ToString());
                yield return Still("fold_element" + s);
                Say($"layout at {size}%, Element folded:");
                Layout();
                C.PointAt(LoadoutPlace.South);
                yield return Real(0.2f);
                Judge($"the Steel half folds with Element in hand at {size}%", C.Folded == LoadoutHalf.Steel, C.Folded.ToString());
                yield return Still("fold_steel" + s);
                Say($"layout at {size}%, Steel folded:");
                Layout();
            }
        }
        C.SetTextSize(100);
        yield return null;
    }

    // ---- The pad walk (7, step 10; 5.3 item 5).
    private IEnumerator PadWalk()
    {
        C.SetTextSize(100);
        C.PointAt(LoadoutPlace.South);
        yield return null;
        var seen = new HashSet<LoadoutPlace> { C.Focus };
        var directions = new[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        // A walk that tries every push from every place reached, until nothing new is reached.
        var queue = new Queue<LoadoutPlace>();
        queue.Enqueue(C.Focus);
        var edges = new StringBuilder("pad moves:");
        while (queue.Count > 0)
        {
            var from = queue.Dequeue();
            foreach (var push in directions)
            {
                C.PointAt(from);
                yield return null;
                yield return Stick(push * 0.9f);
                yield return Real(0.05f);
                var to = C.Focus;
                edges.Append($" {from}{(push == Vector2.up ? "^" : push == Vector2.down ? "v" : push == Vector2.left ? "<" : ">")}{to}");
                if (seen.Add(to)) queue.Enqueue(to);
            }
        }
        Say(edges.ToString());
        var missed = System.Enum.GetValues(typeof(LoadoutPlace)).Cast<LoadoutPlace>().Where(p => !seen.Contains(p) && Root.Q(LoadoutPlaces.Names[(int)p])?.resolvedStyle.display != DisplayStyle.None).ToList();
        Judge("the stick reaches every place", missed.Count == 0, missed.Count == 0 ? $"{seen.Count} places" : "missed " + string.Join(",", missed));

        // A on a slot opens its list; the stick moves in it; A wears; B backs out of a list without leaving.
        C.PointAt(LoadoutPlace.North);
        yield return Pad(GamepadButton.South);
        yield return Real(0.1f);
        Judge("A on a slot opens its list", C.Mode == LoadoutMode.SpellList, C.Mode.ToString());
        int row = C.Row;
        var lean = row > 0 ? Vector2.up : Vector2.down;
        yield return Stick(lean * 0.9f);
        Judge("the stick moves in the list", C.Row != row || C.Columns.Count == 0 || C.Columns[C.Column].Rows.Count < 2, $"row {row} -> {C.Row}");
        yield return Still("pad_list");
        yield return Pad(GamepadButton.East);
        yield return Real(0.1f);
        Judge("B from a list goes back to the board, the Loadout still open", C.Mode == LoadoutMode.Board && LoadoutScreen.Showing, $"{C.Mode}, showing {LoadoutScreen.Showing}");

        // Y on a slot with a spell of two shapes swaps the shape (or says why not).
        C.PointAt(LoadoutPlace.West);
        int version = C.Version;
        yield return Pad(GamepadButton.North);
        yield return Real(0.1f);
        Judge("Y on a slot is heard", C.Version != version, $"notice '{C.Notice}'");

        // The aspect cards: A opens, B backs out.
        C.PointAt(LoadoutPlace.Aspect);
        yield return Pad(GamepadButton.South);
        yield return Real(0.1f);
        Judge("A on the aspect opens its cards", C.Mode == LoadoutMode.AspectCards, C.Mode.ToString());
        yield return Pad(GamepadButton.East);
        yield return Real(0.1f);
        Judge("B from the cards goes back to the board", C.Mode == LoadoutMode.Board && LoadoutScreen.Showing, C.Mode.ToString());

        // LB and RB jump between the halves.
        C.PointAt(LoadoutPlace.South);
        yield return Pad(GamepadButton.LeftShoulder);
        var steel = C.Focus;
        Judge("LB jumps to the Steel half", LoadoutPlaces.InSteel(steel), steel.ToString());
        yield return Pad(GamepadButton.RightShoulder);
        Judge("RB jumps to the Element half", !LoadoutPlaces.InSteel(C.Focus), C.Focus.ToString());

        // L3 steps the text size round.
        int sizeBefore = C.TextSize;
        yield return Pad(GamepadButton.LeftStick);
        Judge("L3 steps the text size", C.TextSize != sizeBefore, $"{sizeBefore} -> {C.TextSize}");
        yield return Still("pad_l3");
        C.SetTextSize(100);

        // A held X on an empty saved Hand keeps the worn Hand there.
        C.PointAt(LoadoutPlace.Hand2);
        bool wasEmpty = C.Board.Hands[2].Empty;
        yield return Pad(GamepadButton.West, 1.2f);
        yield return Real(0.1f);
        Judge("a held X keeps the worn Hand in Hand 2", !C.Board.Hands[2].Empty, $"was empty {wasEmpty}, notice '{C.Notice}'");

        // The element header's A opens the Reactions page over the Loadout; B comes back to the Loadout.
        C.PointAt(LoadoutPlace.ElementHeader);
        yield return Pad(GamepadButton.South);
        yield return Real(0.3f);
        Judge("A on the element header opens the Reactions page", ShapeMenu.Showing, $"loadout shown {Root.Q("loadout").resolvedStyle.display}");
        yield return Still("reactions");
        yield return Pad(GamepadButton.East);
        yield return Real(0.3f);
        Judge("B from the Reactions page comes back to the Loadout", !ShapeMenu.Showing && LoadoutScreen.Showing && Root.Q("loadout").resolvedStyle.display == DisplayStyle.Flex, $"page {ShapeMenu.Showing}, loadout {LoadoutScreen.Showing}");

        // B on the board goes back to the fire.
        yield return Pad(GamepadButton.East);
        yield return Real(0.2f);
        Judge("B on the board closes the Loadout", !LoadoutScreen.Showing);
        if (!LoadoutScreen.Showing) screen.Open();
        yield return Real(0.3f);
    }

    // ---- The mouse walk: a move points, a click acts, a right click is B.
    private IEnumerator MouseWalk()
    {
        var mouse = Mouse.current ?? InputSystem.AddDevice<Mouse>();
        C.SetTextSize(100);
        yield return null;
        IEnumerator MoveTo(VisualElement element)
        {
            var c = element.worldBound.center;
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            var panelSize = Root.panel.visualTree.layout.size;
            float s = Screen.width / panelSize.x;
            var at = new Vector2(c.x * s, Screen.height - c.y * s);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = at });
            yield return null;
            yield return null;
            yield return null;
        }
        IEnumerator Click(int button = 0)
        {
            var state = new MouseState { position = mouse.position.ReadValue() }.WithButton(button == 0 ? UnityEngine.InputSystem.LowLevel.MouseButton.Left : UnityEngine.InputSystem.LowLevel.MouseButton.Right);
            InputSystem.QueueStateEvent(mouse, state);
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = mouse.position.ReadValue() });
            yield return null;
            yield return null;
        }
        // A first move from off the board, so the walk starts from a pointer the panel has seen.
        InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(4f, 4f) });
        yield return null;
        yield return null;
        var missed = new List<string>();
        for (int i = 0; i < LoadoutPlaces.Names.Length; i++)
        {
            var place = Root.Q(LoadoutPlaces.Names[i]);
            if (place == null || place.resolvedStyle.display == DisplayStyle.None) continue;
            yield return MoveTo(place);
            if ((int)C.Focus != i) missed.Add(LoadoutPlaces.Names[i] + "->" + C.Focus);
        }
        Judge("the pointer focuses every place it moves over", missed.Count == 0, string.Join(" ", missed));
        yield return MoveTo(Root.Q("slot-east"));
        yield return Click();
        Judge("a click on a slot opens its list", C.Mode == LoadoutMode.SpellList, C.Mode.ToString());
        yield return Click(1);
        Judge("a right click backs out of the list", C.Mode == LoadoutMode.Board && LoadoutScreen.Showing, C.Mode.ToString());
        yield return MoveTo(Root.Q("aspect"));
        yield return Click();
        Judge("a click on the aspect opens its cards", C.Mode == LoadoutMode.AspectCards, C.Mode.ToString());
        yield return Still("mouse_cards");
        yield return Click(1);
        Judge("a right click backs out of the cards", C.Mode == LoadoutMode.Board, C.Mode.ToString());
        yield return Click(1);
        Judge("a right click on the board closes the Loadout", !LoadoutScreen.Showing);
    }
}
