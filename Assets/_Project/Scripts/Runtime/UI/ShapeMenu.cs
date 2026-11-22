using System.Collections.Generic;
using System.Text;
using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The Reactions page, opened from the Loadout's element header (the owner's answer Q4 of 2026-11-21): each learned
    /// reaction's shape, listed by <see cref="ShapeChoices"/>, the shape worn beside each, and the chosen one's two shapes
    /// in full on the right. Up and down choose a line; left and right, or use, wear the other shape through
    /// <see cref="PlayerBuild.ChooseShape"/>; a click on a line chooses it and a click on a shape wears it; the dodge
    /// button (or Esc, or Start) backs out to the Loadout. Read at the player's own pace: the world stays stopped.
    /// Made by code the first time it is wanted, under the HUD. A spell's shape is chosen on the Loadout (Y on its slot).
    /// </summary>
    public class ShapeMenu : MonoBehaviour
    {
        public static ShapeMenu Instance { get; private set; }
        public static bool Showing => Instance != null && Instance.IsOpen;
        /// <summary>True while open or closed this frame: the rest menu under it leaves its buttons alone then.</summary>
        public static bool BusyThisFrame => Instance != null && (Instance.IsOpen || Instance.closedOnFrame == Time.frameCount);

        public float RepeatSeconds = 0.2f;

        public bool IsOpen { get; private set; }
        public ShapeChoices Choices { get; } = new ShapeChoices();
        // Counts for probes.
        public int Opened { get; private set; }
        public int Worn { get; private set; }

        private PlayerController player;
        private PlayerBuild build;
        private InputAction moveAction, interactAction, dodgeAction, pauseAction, alwaysBack;
        private Text title, empty, infoTitle, infoNote, hint;
        private RectTransform listCursor;
        private readonly List<Text> listRows = new List<Text>();
        private readonly Text[] shapeNames = new Text[2], shapeWords = new Text[2];
        private readonly Image[] shapeBoxes = new Image[2], shapeBacks = new Image[2];
        private readonly StringBuilder words = new StringBuilder(256);
        private int openedOnFrame = -1, closedOnFrame = -1;
        private float savedScale = 1f;
        private bool savedSuspended;
        private int heldVertical, heldSideways;
        private float untilRepeat;
        private int firstRow;
        private int shownVersion = -1, hintVersion = -1;

        private const int VisibleRows = 13;
        private const float RowHeight = 50f;
        private const float ListTop = -120f;
        private static readonly Color ChosenInk = new Color(0.16f, 0.1f, 0.04f, 1f);

        // Domain reload is off: the screen of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>The shape screen for this scene, made under the HUD the first time it is asked for. Null when the scene has no HUD or player.</summary>
        public static ShapeMenu Ensure()
        {
            if (Instance != null) return Instance;
            var hud = MenuParts.FindHud();
            var player = FindAnyObjectByType<PlayerController>();
            if (hud == null || player == null) return null;
            var made = new GameObject("Reactions Page", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            var menu = made.AddComponent<ShapeMenu>();
            menu.Build(hud, player);
            Instance = menu;
            return menu;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            // A scene change with the screen open must not leave the world stopped.
            if (IsOpen)
            {
                IsOpen = false;
                Time.timeScale = savedScale > 0f ? savedScale : 1f;
                if (player != null) player.Suspended = false;
            }
            if (alwaysBack == null) return;
            alwaysBack.Disable();
            alwaysBack.Dispose();
            alwaysBack = null;
        }

        private void Build(Transform hud, PlayerController owner)
        {
            player = owner;
            build = owner.GetComponent<PlayerBuild>();
            moveAction = player.FindAction("Move");
            interactAction = player.FindAction("Interact");
            dodgeAction = player.FindAction("Dodge");
            pauseAction = player.FindAction("Pause");
            alwaysBack = new InputAction("Reactions Back", InputActionType.Button);
            foreach (string path in ControlBindings.Reserved) alwaysBack.AddBinding(path);
            alwaysBack.Enable();
            MenuParts.FindLook(hud, out var font, out var frame, out var brush);

            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.008f, 0.03f, 0.93f);
            dim.raycastTarget = false;
            var backing = MenuParts.Rect("Backing", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1612f, 892f));
            var backingImage = backing.gameObject.AddComponent<Image>();
            backingImage.color = MenuParts.Ink;
            backingImage.raycastTarget = false;
            var panel = MenuParts.Rect("Panel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1640f, 920f));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = frame;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = frame != null ? Color.white : new Color(0.03f, 0.025f, 0.06f, 1f);
            panelImage.raycastTarget = false;
            MenuParts.Box("Edge", panel, new Vector2(40f, -86f), new Vector2(1560f, 2f), MenuParts.Rule);
            title = MenuParts.Label("Title", panel, font, 40, TextAnchor.UpperLeft, MenuParts.Gold, new Vector2(44f, -26f), new Vector2(1000f, 50f));
            title.text = "Reactions";

            // The list, on the left: the reaction, and the shape worn.
            listCursor = (RectTransform)MenuParts.Box("Cursor", panel, new Vector2(36f, ListTop), new Vector2(780f, RowHeight - 4f), new Color(1f, 0.62f, 0.2f, 0.3f), brush).transform;
            for (int i = 0; i < VisibleRows; i++)
            {
                var row = MenuParts.Label("Row " + (i + 1), panel, font, 25, TextAnchor.MiddleLeft, MenuParts.Plain, new Vector2(52f, ListTop - i * RowHeight), new Vector2(760f, RowHeight - 4f));
                row.supportRichText = true;
                listRows.Add(row);
            }
            empty = MenuParts.Label("Empty", panel, font, 26, TextAnchor.UpperLeft, MenuParts.Faint, new Vector2(52f, -130f), new Vector2(760f, 300f));
            empty.horizontalOverflow = HorizontalWrapMode.Wrap;
            empty.text = "No reaction shape is learned yet. A duo branch's Shape mark on the chart opens two shapes of its reaction. Each is chosen here, at any fire.";

            // The chosen line's two shapes, on the right, the worn one lit.
            infoTitle = MenuParts.Label("Info Title", panel, font, 32, TextAnchor.UpperLeft, MenuParts.Gold, new Vector2(860f, -110f), new Vector2(740f, 44f));
            infoNote = MenuParts.Label("Info Note", panel, font, 21, TextAnchor.UpperLeft, MenuParts.Faint, new Vector2(860f, -158f), new Vector2(740f, 60f));
            infoNote.horizontalOverflow = HorizontalWrapMode.Wrap;
            for (int i = 0; i < 2; i++)
            {
                float top = -230f - i * 300f;
                shapeBoxes[i] = MenuParts.Box("Shape " + (i + 1), panel, new Vector2(860f, top), new Vector2(740f, 280f), MenuParts.Faint);
                shapeBacks[i] = MenuParts.Box("Back", shapeBoxes[i].transform, new Vector2(4f, -4f), new Vector2(732f, 272f), MenuParts.Ink);
                shapeNames[i] = MenuParts.Label("Name", shapeBoxes[i].transform, font, 28, TextAnchor.UpperLeft, MenuParts.Plain, new Vector2(20f, -14f), new Vector2(700f, 40f));
                shapeWords[i] = MenuParts.Label("Words", shapeBoxes[i].transform, font, 22, TextAnchor.UpperLeft, MenuParts.Plain, new Vector2(20f, -62f), new Vector2(700f, 200f));
                shapeWords[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                shapeWords[i].verticalOverflow = VerticalWrapMode.Truncate;
                shapeWords[i].lineSpacing = 1.1f;
            }
            hint = MenuParts.Label("Hint", panel, font, 22, TextAnchor.LowerLeft, MenuParts.Faint, new Vector2(48f, -876f), new Vector2(1560f, 30f));

            gameObject.SetActive(false);
            transform.SetAsLastSibling();
        }

        /// <summary>Opens the screen (at a rest fire: it is only ever opened from the rest menu). False when there is no build.</summary>
        public bool Open()
        {
            if (player == null) return false;
            if (build == null) build = player.GetComponent<PlayerBuild>();
            if (build == null) return false;
            Opened++;
            if (!IsOpen)
            {
                savedScale = Time.timeScale;
                savedSuspended = player.Suspended;
            }
            IsOpen = true;
            openedOnFrame = Time.frameCount;
            heldVertical = heldSideways = 0;
            firstRow = 0;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            player.Suspended = true;
            Time.timeScale = 0f;
            Redraw();
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            closedOnFrame = Time.frameCount;
            gameObject.SetActive(false);
            Time.timeScale = savedScale;
            player.Suspended = savedSuspended;
        }

        /// <summary>Moves the choice of line.</summary>
        public void Move(int by)
        {
            if (!IsOpen) return;
            Choices.Move(by);
            Redraw();
        }

        /// <summary>Wears the chosen line's next (or previous) shape. False when there is nothing to change.</summary>
        public bool Step(int by)
        {
            if (!IsOpen || Choices.Count == 0) return false;
            return Wear(Choices.Step(by));
        }

        /// <summary>Wears the chosen line's shape at <paramref name="index"/>, as a rest fire allows. False when refused.</summary>
        public bool Wear(int index)
        {
            if (!IsOpen || Choices.Count == 0) return false;
            var row = Choices.Current;
            if (index < 0 || index >= row.ShapeCount) return false;
            bool done = build.ChooseShape(row.Key, index, true);
            if (done) Worn++;
            Redraw();
            return done;
        }

        private void Redraw()
        {
            shownVersion = build.Version;
            Choices.Fill(Upgrade.All, RunState.Unlocked, build.Read());
            bool any = Choices.Count > 0;
            empty.enabled = !any;
            listCursor.gameObject.SetActive(any);
            if (Choices.Selected < firstRow) firstRow = Choices.Selected;
            if (Choices.Selected >= firstRow + VisibleRows) firstRow = Choices.Selected - VisibleRows + 1;
            for (int i = 0; i < listRows.Count; i++)
            {
                int index = firstRow + i;
                bool used = index < Choices.Count;
                listRows[i].enabled = used;
                if (!used) continue;
                var row = Choices.At(index);
                var worn = row.WornShape;
                words.Clear();
                words.Append(row.Title).Append("   ");
                words.Append(worn != null ? worn.Name : "");
                if (!row.InForce) words.Append("   <color=#8C8C8C>(both elements not carried)</color>");
                listRows[i].text = words.ToString();
                listRows[i].color = index == Choices.Selected ? MenuParts.Gold : MenuParts.Plain;
            }
            if (any) listCursor.anchoredPosition = new Vector2(36f, ListTop - (Choices.Selected - firstRow) * RowHeight);
            ShowChosen(any);
            ShowHint();
        }

        private void ShowChosen(bool any)
        {
            infoTitle.enabled = infoNote.enabled = any;
            for (int i = 0; i < 2; i++) shapeBoxes[i].gameObject.SetActive(any && i < (any ? Choices.Current.ShapeCount : 0));
            if (!any) return;
            var row = Choices.Current;
            infoTitle.text = row.Title + ": the reaction's shape";
            infoNote.text = "The shape runs instead of the plain reaction while both its elements are carried.";
            for (int i = 0; i < 2 && i < row.ShapeCount; i++)
            {
                var shape = row.Mark.Shapes[i];
                bool worn = i == row.Worn;
                shapeNames[i].text = worn ? shape.Name + "   (worn)" : shape.Name;
                shapeNames[i].color = worn ? MenuParts.Gold : MenuParts.Plain;
                shapeWords[i].text = shape.Description;
                shapeBoxes[i].color = worn ? MenuParts.Gold : MenuParts.Faint;
                shapeBacks[i].color = worn ? ChosenInk : MenuParts.Ink;
            }
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            bool keys = ControlDevices.Current == ControlDevice.Keys;
            string choose = keys ? "W S, the wheel or the mouse" : "Stick or d-pad up and down";
            string swap = keys ? "A D" : "left and right";
            hint.text = $"{choose}: choose     {swap}, {ControlNames.Of("Interact")} or a click: wear the other shape     {ControlNames.Of("Dodge")}: back to the Loadout";
        }

        // ---- The buttons.

        private void Update()
        {
            if (!IsOpen) return;
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            if (GuidePage.BusyThisFrame || ReadingBox.BusyThisFrame) return;
            if (build.Version != shownVersion) Redraw();
            else if (ControlDevices.Version != hintVersion) ShowHint();
            if (dodgeAction.WasPressedThisFrame() || (pauseAction != null && pauseAction.WasPressedThisFrame()) || (alwaysBack != null && alwaysBack.WasPressedThisFrame()))
            {
                Close();
                return;
            }
            if (interactAction.WasPressedThisFrame())
            {
                Step(1);
                return;
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel != 0f) Move(wheel > 0f ? -1 : 1);
                if (mouse.leftButton.wasPressedThisFrame && Pick(mouse.position.ReadValue())) return;
            }
            var stick = moveAction.ReadValue<Vector2>();
            bool upright = Mathf.Abs(stick.y) >= Mathf.Abs(stick.x);
            int vertical = upright ? (stick.y > 0.5f ? -1 : stick.y < -0.5f ? 1 : 0) : 0;
            int sideways = upright ? 0 : (stick.x > 0.5f ? 1 : stick.x < -0.5f ? -1 : 0);
            // Left and right wear the other shape once a push; up and down repeat while held, as in the Hand.
            if (sideways != heldSideways)
            {
                heldSideways = sideways;
                if (sideways != 0) Step(sideways);
            }
            if (vertical != heldVertical)
            {
                heldVertical = vertical;
                untilRepeat = RepeatSeconds * 1.6f;
                if (vertical != 0) Move(vertical);
            }
            else if (vertical != 0)
            {
                untilRepeat -= Time.unscaledDeltaTime;
                if (untilRepeat > 0f) return;
                untilRepeat = RepeatSeconds;
                Move(vertical);
            }
        }

        // A click on a line chooses it; a click on a shape wears it. True when it landed on one.
        private bool Pick(Vector2 at)
        {
            for (int i = 0; i < listRows.Count; i++)
            {
                int index = firstRow + i;
                if (!listRows[i].enabled || index >= Choices.Count || !MenuParts.Contains(listRows[i].rectTransform, at)) continue;
                Choices.Choose(index);
                Redraw();
                return true;
            }
            for (int i = 0; i < 2; i++)
            {
                if (!MenuParts.Contains((RectTransform)shapeBoxes[i].transform, at)) continue;
                Wear(i);
                return true;
            }
            return false;
        }
    }
}
