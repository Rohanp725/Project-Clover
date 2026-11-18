using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The controls page of the pause menu: every action in a row, with what it is bound to on the pad, on the keys and
    /// mouse, and in the keys-only set. The stick or the keys choose a cell, the use button (or a click) listens for a
    /// new button through the Input System's interactive rebinding, and a clash is settled by swapping. The bottom row
    /// puts everything back. Changes are kept at once (<see cref="ControlBindings"/>). It is laid out by code on the page
    /// the pause menu's builder made, so scenes need no rebuild; the page's old fixed columns are hidden.
    /// </summary>
    public class ControlsPage : MonoBehaviour
    {
        private struct Row
        {
            public string Label, Action, Part;
            public Row(string label, string action, string part = null)
            {
                Label = label;
                Action = action;
                Part = part;
            }
        }

        private static readonly Row[] Rows =
        {
            new Row("Move up", "Move", "up"),
            new Row("Move down", "Move", "down"),
            new Row("Move left", "Move", "left"),
            new Row("Move right", "Move", "right"),
            new Row("Light attack", "LightAttack"),
            new Row("Heavy attack", "HeavyAttack"),
            new Row("Guard", "Guard"),
            new Row("Dodge", "Dodge"),
            new Row("Flask", "Flask"),
            new Row("Spell", "Spell"),
            new Row("Surge", "Surge"),
            new Row("Use", "Interact"),
            new Row("Pause", "Pause"),
        };

        private const int Columns = 3;
        private static readonly float[] ColumnLeft = { 286f, 466f, 676f };
        private static readonly float[] ColumnWidth = { 170f, 200f, 118f };
        private const float Top = -112f;
        private const float RowHeight = 35f;
        /// <summary>The row under the actions that puts every binding back.</summary>
        public static int ResetRow => Rows.Length;

        public Color Plain = new Color(0.95f, 0.93f, 0.9f, 1f);
        public Color Chosen = new Color(1f, 0.8f, 0.45f, 1f);
        public Color Faint = new Color(1f, 1f, 1f, 0.45f);
        public float RepeatSeconds = 0.2f;
        /// <summary>Real seconds a rebind listens before it gives up.</summary>
        public float ListenSeconds = 8f;
        /// <summary>Real seconds between the press that asks for a rebind and the listening, so that press is not taken.</summary>
        public float ArmSeconds = 0.2f;

        public int SelectedRow { get; private set; }
        public int Column { get; private set; }
        /// <summary>True from the press that asks for a rebind until the buttons are given back after it.</summary>
        public bool Busy => arming || operation != null || finished != 0 || giveBackIn > 0;
        public bool Listening => operation != null;
        public string Message { get; private set; } = "";
        public int Rebinds { get; private set; }
        public int Resets { get; private set; }
        public bool AskingReset { get; private set; }

        private PlayerController player;
        private Text[] names;
        private Text[,] cells;
        private Text[] headers;
        private Text reset;
        private Text status;
        private RectTransform cursor;
        private bool built;
        private int shownVersion = -1;
        private int heldX, heldY;
        private float untilRepeat;
        private bool arming;
        private float armLeft;
        private InputActionRebindingExtensions.RebindingOperation operation;
        private int finished;
        private int giveBackIn;
        private InputAction listenAction;
        private int listenIndex;
        private string listenOld;
        private bool holding;

        /// <summary>Lays the page out over the one the builder made, in the same type and colours as its words.</summary>
        public void Setup(PlayerController playerToServe, Text style, Sprite cursorSprite)
        {
            player = playerToServe;
            if (built || style == null) return;
            built = true;
            // The builder's fixed columns name PlayStation buttons and the old keys; the fight's words stay, moved right.
            foreach (string old in new[] { "Names", "Pad", "Keys" })
            {
                var column = transform.Find(old);
                if (column != null) column.gameObject.SetActive(false);
            }
            Place("Fight", 840f, -112f, 440f, 300f);
            Place("Holds", 840f, -500f, 440f, 220f);
            Place("Rule", 808f, -112f, 2f, 600f);
            var fight = transform.Find("Fight")?.GetComponent<Text>();
            if (fight != null)
            {
                fight.fontSize = Mathf.RoundToInt(style.fontSize * 0.8f);
                fight.lineSpacing = 1f;
            }
            var holds = transform.Find("Holds")?.GetComponent<Text>();
            if (holds != null)
            {
                holds.horizontalOverflow = HorizontalWrapMode.Wrap;
                holds.fontSize = Mathf.RoundToInt(style.fontSize * 0.8f);
                // Scenes built before the skill's price became per weapon still say thirty for all of them.
                holds.text = holds.text.Replace("the weapon's skill, 30 mana", "the weapon's skill, costs mana");
            }

            var cursorObject = new GameObject("Cell Cursor", typeof(RectTransform));
            cursorObject.transform.SetParent(transform, false);
            cursor = (RectTransform)cursorObject.transform;
            cursor.anchorMin = cursor.anchorMax = cursor.pivot = new Vector2(0f, 1f);
            var image = cursorObject.AddComponent<Image>();
            image.sprite = cursorSprite;
            image.type = cursorSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = new Color(1f, 0.62f, 0.2f, 0.3f);
            image.raycastTarget = false;

            headers = new Text[Columns];
            for (int c = 0; c < Columns; c++) headers[c] = Make("Header " + c, style, ColumnLeft[c], Top, ColumnWidth[c], Chosen);
            names = new Text[Rows.Length];
            cells = new Text[Rows.Length, Columns];
            for (int r = 0; r < Rows.Length; r++)
            {
                float y = Top - (r + 1) * RowHeight;
                names[r] = Make("Name " + r, style, 48f, y, 236f, Plain);
                names[r].text = Rows[r].Label;
                for (int c = 0; c < Columns; c++) cells[r, c] = Make($"Cell {r} {c}", style, ColumnLeft[c], y, ColumnWidth[c], Plain);
            }
            reset = Make("Reset", style, 48f, Top - (Rows.Length + 1) * RowHeight - 10f, 700f, Plain);
            status = Make("Status", style, 48f, Top - (Rows.Length + 2) * RowHeight - 14f, 760f, Chosen);
            status.horizontalOverflow = HorizontalWrapMode.Wrap;
            status.fontSize = Mathf.RoundToInt(style.fontSize * 0.78f);
            SelectedRow = 4;
            Column = Current == ControlDevice.Keys ? 1 : 0;
        }

        private void Place(string child, float x, float y, float width, float height)
        {
            var found = transform.Find(child) as RectTransform;
            if (found == null) return;
            found.anchorMin = found.anchorMax = found.pivot = new Vector2(0f, 1f);
            found.anchoredPosition = new Vector2(x, y);
            found.sizeDelta = new Vector2(width, height);
        }

        private Text Make(string name, Text style, float x, float y, float width, Color colour)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(transform, false);
            var label = labelObject.AddComponent<Text>();
            label.font = style.font;
            // The note's type, a little smaller, so four columns sit side by side.
            label.fontSize = Mathf.RoundToInt(style.fontSize * 0.85f);
            label.alignment = TextAnchor.MiddleLeft;
            label.color = colour;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var styleOutline = style.GetComponent<Outline>();
            if (styleOutline != null)
            {
                var outline = labelObject.AddComponent<Outline>();
                outline.effectColor = styleOutline.effectColor;
                outline.effectDistance = styleOutline.effectDistance;
            }
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, RowHeight);
            return label;
        }

        private static ControlDevice Current => ControlDevices.Current;
        private InputActionAsset Controls => player != null ? player.Controls : null;

        /// <summary>The action and binding a cell stands for; false for an empty cell.</summary>
        private bool Target(int row, int column, out InputAction action, out int index)
        {
            action = null;
            index = -1;
            var controls = Controls;
            if (controls == null || row < 0 || row >= Rows.Length || column < 0 || column >= Columns) return false;
            var shape = Rows[row];
            action = controls.FindAction(shape.Action);
            if (action == null) return false;
            if (column == 0)
            {
                // The pad walks with one stick; its row is the first of the four.
                if (shape.Part != null && shape.Part != "up") return false;
                index = ControlBindings.IndexOf(action, ControlBindings.PadGroup);
            }
            else if (shape.Part != null) index = column == 1 ? ControlBindings.IndexOf(action, ControlBindings.KeysGroup, shape.Part) : -1;
            else index = ControlBindings.IndexOf(action, ControlBindings.KeysGroup, null, column - 1);
            return index >= 0;
        }

        /// <summary>What a cell says now, in the names of its column's device.</summary>
        public string CellText(int row, int column)
        {
            if (!Target(row, column, out InputAction action, out int index)) return "";
            var style = column == 0 ? ControlDevices.Pad : ControlDevice.Keys;
            return ControlNames.OfPath(action.bindings[index].effectivePath, style);
        }

        private string LabelOf(InputAction action, int index)
        {
            var binding = action.bindings[index];
            for (int r = 0; r < Rows.Length; r++)
            {
                if (Rows[r].Action != action.name) continue;
                if (Rows[r].Part == null || (binding.isPartOfComposite && binding.name == Rows[r].Part)) return Rows[r].Label;
            }
            return action.name;
        }

        public void Show()
        {
            if (!built) return;
            shownVersion = ControlDevices.Version;
            headers[0].text = ControlDevices.Pad == ControlDevice.PlayStation ? "PlayStation" : "Xbox pad";
            headers[1].text = "Keys and mouse";
            headers[2].text = "Keys only";
            for (int r = 0; r < Rows.Length; r++)
            {
                names[r].color = r == SelectedRow ? Chosen : Plain;
                for (int c = 0; c < Columns; c++)
                {
                    bool listening = (arming || operation != null) && r == SelectedRow && c == Column;
                    cells[r, c].text = listening ? "press..." : CellText(r, c);
                    cells[r, c].color = r == SelectedRow && c == Column ? Chosen : Plain;
                }
            }
            reset.text = AskingReset ? "Put every binding back as it came? Press again to do it." : "Reset to defaults";
            reset.color = SelectedRow == ResetRow ? Chosen : Plain;
            status.text = Message;
            if (SelectedRow == ResetRow)
            {
                cursor.anchoredPosition = new Vector2(30f, reset.rectTransform.anchoredPosition.y + 2f);
                cursor.sizeDelta = new Vector2(740f, RowHeight + 4f);
            }
            else
            {
                cursor.anchoredPosition = new Vector2(ColumnLeft[Column] - 14f, cells[SelectedRow, Column].rectTransform.anchoredPosition.y + 2f);
                cursor.sizeDelta = new Vector2(ColumnWidth[Column] + 10f, RowHeight + 4f);
            }
        }

        /// <summary>The words for the hint line while this page is open.</summary>
        public string Hint()
        {
            if (arming || operation != null)
                return Column == 0
                    ? $"Press the pad's button for {Rows[SelectedRow].Label}.     {ControlNames.Pad("<Gamepad>/start", ControlDevices.Pad)}: leave it as it was"
                    : $"Press a key or mouse button for {Rows[SelectedRow].Label}.     Esc: leave it as it was";
            var style = Current;
            string choose = style == ControlDevice.Keys ? "W A S D or the mouse" : "Stick or d-pad";
            return $"{choose}: choose     {ControlNames.Of("Interact")}: change     {ControlNames.Of("Dodge")}: back     "
                + $"{(style == ControlDevice.Keys ? "Esc" : ControlNames.Pad("<Gamepad>/start", style))}: resume";
        }

        /// <summary>Moves the choice by whole cells, skipping empty ones; down from the last action is the reset row.</summary>
        public void Step(int across, int down)
        {
            if (Busy) return;
            AskingReset = false;
            if (down != 0)
            {
                int row = Mathf.Clamp(SelectedRow + down, 0, ResetRow);
                if (row != ResetRow)
                {
                    // Into a row that has nothing in this column: the nearest column that has something.
                    int column = Column;
                    if (!Target(row, column, out _, out _))
                    {
                        column = -1;
                        for (int c = 0; c < Columns && column < 0; c++)
                            if (Target(row, c, out _, out _)) column = c;
                    }
                    if (column < 0) return;
                    Column = column;
                }
                SelectedRow = row;
            }
            else if (across != 0 && SelectedRow != ResetRow)
            {
                for (int c = Column + across; c >= 0 && c < Columns; c += across)
                {
                    if (!Target(SelectedRow, c, out _, out _)) continue;
                    Column = c;
                    break;
                }
            }
            Show();
        }

        /// <summary>Chooses a cell outright (a click); false when it is empty.</summary>
        public bool Choose(int row, int column)
        {
            if (Busy) return false;
            if (row == ResetRow)
            {
                if (SelectedRow != ResetRow) AskingReset = false;
                SelectedRow = row;
                Show();
                return true;
            }
            if (!Target(row, column, out _, out _)) return false;
            if (SelectedRow != row || Column != column) AskingReset = false;
            SelectedRow = row;
            Column = column;
            Show();
            return true;
        }

        /// <summary>The use button: listens for a new binding for the chosen cell, or, on the bottom row, resets (asking first).</summary>
        public void Pick()
        {
            if (Busy) return;
            if (SelectedRow == ResetRow)
            {
                if (!AskingReset)
                {
                    AskingReset = true;
                    Show();
                    return;
                }
                AskingReset = false;
                ControlBindings.ResetToDefaults(Controls);
                Resets++;
                Message = "Every binding is back as the game came.";
                Show();
                return;
            }
            if (!Target(SelectedRow, Column, out listenAction, out listenIndex)) return;
            arming = true;
            armLeft = ArmSeconds;
            Message = "";
            Show();
        }

        private void Listen()
        {
            arming = false;
            listenOld = listenAction.bindings[listenIndex].effectivePath;
            // The action must be quiet while it is rebound, and the menus must not hear the press that is being taken.
            player.HoldInput(true);
            holding = true;
            bool pad = Column == 0;
            var rebind = listenAction.PerformInteractiveRebinding(listenIndex)
                .WithTimeout(ListenSeconds)
                .OnMatchWaitForAnother(0.08f)
                .OnComplete(_ => finished = 1)
                .OnCancel(_ => finished = 2);
            if (pad)
            {
                rebind.WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start");
            }
            else
            {
                rebind.WithControlsHavingToMatchPath("<Keyboard>").WithControlsHavingToMatchPath("<Mouse>").WithCancelingThrough("<Keyboard>/escape");
                rebind.WithControlsExcluding("<Keyboard>/anyKey").WithControlsExcluding("<Mouse>/scroll").WithControlsExcluding("<Mouse>/scroll/*");
            }
            if (pad && Rows[SelectedRow].Action == "Move")
            {
                // The pad walks with a stick; the d-pad always walks as well.
                rebind.WithExpectedControlType("Stick");
            }
            else
            {
                rebind.WithExpectedControlType("Button");
                // Escape and Start always pause; the sticks and the d-pad walk and choose.
                foreach (string kept in ControlBindings.Reserved) rebind.WithControlsExcluding(kept);
                rebind.WithControlsExcluding("<Gamepad>/dpad/*").WithControlsExcluding("<Gamepad>/leftStick/*").WithControlsExcluding("<Gamepad>/rightStick/*");
            }
            operation = rebind.Start();
            Show();
        }

        private void Finish()
        {
            bool done = finished == 1;
            finished = 0;
            if (operation != null)
            {
                operation.Dispose();
                operation = null;
            }
            string label = LabelOf(listenAction, listenIndex);
            var style = Column == 0 ? ControlDevices.Pad : ControlDevice.Keys;
            if (!done) Message = $"{label}: nothing was changed.";
            else
            {
                string now = listenAction.bindings[listenIndex].effectivePath;
                var settled = ControlBindings.Settle(listenAction.actionMap, listenAction, listenIndex, listenOld, out InputAction other, out int otherIndex);
                string named = ControlNames.OfPath(now, style);
                switch (settled)
                {
                    case ControlBindings.Settled.Swapped:
                        Message = $"{label}: now {named}. {LabelOf(other, otherIndex)} had it, and takes {ControlNames.OfPath(listenOld, style)} instead.";
                        break;
                    case ControlBindings.Settled.Refused:
                        Message = $"{named} is {LabelOf(other, otherIndex)}'s, and {ControlNames.OfPath(listenOld, style)} cannot be handed on. {label} keeps {ControlNames.OfPath(listenOld, style)}.";
                        break;
                    default:
                        Message = $"{label}: now {named}.";
                        break;
                }
                ControlBindings.Save(Controls);
                Rebinds++;
            }
            // The buttons come back two frames on, so the press just taken is not also heard by the menu.
            giveBackIn = 2;
            Show();
        }

        /// <summary>
        /// One frame of the page. <paramref name="stick"/> chooses; <paramref name="pick"/> and <paramref name="back"/>
        /// are this frame's presses of use and dodge. True when the page should be left.
        /// </summary>
        public bool Tick(Vector2 stick, bool pick, bool back)
        {
            if (!built) return back;
            if (finished != 0)
            {
                Finish();
                return false;
            }
            if (giveBackIn > 0)
            {
                if (--giveBackIn == 0 && player != null)
                {
                    player.HoldInput(false);
                    holding = false;
                }
                return false;
            }
            if (operation != null) return false;
            if (arming)
            {
                armLeft -= Time.unscaledDeltaTime;
                var held = Mouse.current;
                if (armLeft <= 0f && (held == null || !held.leftButton.isPressed)) Listen();
                return false;
            }
            if (shownVersion != ControlDevices.Version) Show();

            var mouse = Mouse.current;
            if (mouse != null)
            {
                bool moved = mouse.delta.ReadValue().sqrMagnitude > 1f;
                bool clicked = mouse.leftButton.wasPressedThisFrame;
                if ((moved || clicked) && CellAt(mouse.position.ReadValue(), out int row, out int column))
                {
                    bool chose = Choose(row, column);
                    if (clicked && chose)
                    {
                        Pick();
                        return false;
                    }
                }
            }

            int x = stick.x > 0.5f ? 1 : stick.x < -0.5f ? -1 : 0;
            int y = stick.y > 0.5f ? -1 : stick.y < -0.5f ? 1 : 0;
            if (x != heldX || y != heldY)
            {
                heldX = x;
                heldY = y;
                untilRepeat = RepeatSeconds * 1.6f;
                if (y != 0) Step(0, y);
                else if (x != 0) Step(x, 0);
            }
            else if (x != 0 || y != 0)
            {
                untilRepeat -= Time.unscaledDeltaTime;
                if (untilRepeat <= 0f)
                {
                    untilRepeat = RepeatSeconds;
                    if (y != 0) Step(0, y);
                    else Step(x, 0);
                }
            }

            if (pick) Pick();
            else if (back)
            {
                if (!AskingReset) return true;
                AskingReset = false;
                Show();
            }
            return false;
        }

        private bool CellAt(Vector2 screen, out int row, out int column)
        {
            row = column = -1;
            if (reset != null && RectTransformUtility.RectangleContainsScreenPoint(reset.rectTransform, screen, null))
            {
                row = ResetRow;
                return true;
            }
            for (int r = 0; r < Rows.Length; r++)
                for (int c = 0; c < Columns; c++)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(cells[r, c].rectTransform, screen, null)) continue;
                    row = r;
                    column = c;
                    return true;
                }
            return false;
        }

        /// <summary>Starts the page afresh each time it is opened: nothing asked, the message cleared.</summary>
        public void Opened()
        {
            AskingReset = false;
            Message = "";
            heldX = heldY = 0;
            Show();
        }

        /// <summary>For probes: chooses a cell and asks for it to be rebound, as the use button does.</summary>
        public void Rebind(int row, int column)
        {
            if (Choose(row, column)) Pick();
        }

        private void OnDisable()
        {
            // Leaving mid-rebind (the menu shut, the scene gone) gives the buttons back and changes nothing.
            if (operation != null)
            {
                operation.Cancel();
                operation.Dispose();
                operation = null;
            }
            arming = false;
            finished = 0;
            giveBackIn = 0;
            if (holding && player != null) player.HoldInput(false);
            holding = false;
        }
    }
}
