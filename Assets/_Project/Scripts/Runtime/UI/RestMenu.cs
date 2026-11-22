using System.Collections.Generic;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// What a rest fire opens (IMPLEMENTATION.md 4.1): set the Hand, choose the shapes, the chart, and leave (the Way and the road to the
    /// hub join when they are built). The entries are <see cref="RestChoices"/>; this draws them and reads the buttons.
    /// The stick, the d-pad, the keys, the mouse and its wheel choose; the use button (or a click) acts; the dodge
    /// button (or Esc, or Start) leaves. The world stays stopped while it is open, and while the Hand or the chart opened
    /// from it is open; when either closes, the rest menu is back. Made by code the first time it is wanted, under the
    /// HUD of whatever scene the player is in, as <see cref="GuidePage"/> is, so no scene had to be built again.
    /// </summary>
    public class RestMenu : MonoBehaviour
    {
        public static RestMenu Instance { get; private set; }
        /// <summary>True while the menu, or a screen opened from it, has the player.</summary>
        public static bool Showing => Instance != null && Instance.IsOpen;

        public float RepeatSeconds = 0.2f;

        public bool IsOpen { get; private set; }
        /// <summary>True while the Hand or the chart opened from here is in front.</summary>
        public bool Away => waiting != RestEntry.None;
        public RestChoices Choices { get; private set; } = new RestChoices();
        public int Opened { get; private set; }
        /// <summary>The fire it was opened at.</summary>
        public Component Fire { get; private set; }

        private PlayerController player;
        private InputAction moveAction, interactAction, dodgeAction, pauseAction, alwaysBack;
        private RectTransform panel, cursor;
        private Text title, note, hint;
        private readonly List<Text> rows = new List<Text>();
        private RestEntry waiting = RestEntry.None;
        private int openedOnFrame = -1;
        private float savedScale = 1f;
        private bool savedSuspended;
        private bool releasePlayer;
        private int heldDirection;
        private float untilRepeat;
        private int hintVersion = -1;

        private const float RowHeight = 54f;
        private const float ListTop = -110f;
        private const int MostRows = 5;

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>The menu for this scene, made under the HUD the first time it is asked for. Null when the scene has no HUD.</summary>
        public static RestMenu Ensure()
        {
            if (Instance != null) return Instance;
            var hud = MenuParts.FindHud();
            var player = FindAnyObjectByType<PlayerController>();
            if (hud == null || player == null) return null;
            var made = new GameObject("Rest Menu", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            var menu = made.AddComponent<RestMenu>();
            menu.Build(hud, player);
            Instance = menu;
            return menu;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            // A scene change with the menu open must not leave the world stopped or the player frozen.
            if (IsOpen || releasePlayer)
            {
                if (IsOpen) Time.timeScale = savedScale;
                if (player != null) player.Suspended = savedSuspended;
                IsOpen = false;
                releasePlayer = false;
            }
            if (alwaysBack == null) return;
            alwaysBack.Disable();
            alwaysBack.Dispose();
            alwaysBack = null;
        }

        private void Build(Transform hud, PlayerController owner)
        {
            player = owner;
            moveAction = player.FindAction("Move");
            interactAction = player.FindAction("Interact");
            dodgeAction = player.FindAction("Dodge");
            pauseAction = player.FindAction("Pause");
            alwaysBack = new InputAction("Rest Back", InputActionType.Button);
            foreach (string path in ControlBindings.Reserved) alwaysBack.AddBinding(path);
            alwaysBack.Enable();
            MenuParts.FindLook(hud, out var font, out var frame, out var brush);

            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.008f, 0.03f, 0.55f);
            dim.raycastTarget = false;

            // A narrow panel to the left of the fire, so the fire and the player still show beside it.
            // Ink under the frame, as under the Hand's, so the floor's own words do not show through the panel's.
            var under = MenuParts.Rect("Backing", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(134f, 0f), new Vector2(592f, 542f));
            var underImage = under.gameObject.AddComponent<Image>();
            underImage.color = new Color(MenuParts.Ink.r, MenuParts.Ink.g, MenuParts.Ink.b, 0.94f);
            underImage.raycastTarget = false;
            panel = MenuParts.Rect("Panel", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(620f, 570f));
            var backing = panel.gameObject.AddComponent<Image>();
            backing.sprite = frame;
            backing.type = Image.Type.Sliced;
            backing.color = frame != null ? Color.white : MenuParts.Ink;
            backing.raycastTarget = false;
            title = MenuParts.Label("Title", panel, font, 40, TextAnchor.UpperLeft, MenuParts.Gold, new Vector2(40f, -26f), new Vector2(540f, 50f));
            title.text = "Rest";
            MenuParts.Box("Edge", panel, new Vector2(36f, -84f), new Vector2(548f, 2f), MenuParts.Rule);
            cursor = (RectTransform)MenuParts.Box("Cursor", panel, new Vector2(26f, ListTop), new Vector2(568f, RowHeight - 6f), new Color(1f, 0.62f, 0.2f, 0.3f), brush).transform;
            for (int i = 0; i < MostRows; i++)
                rows.Add(MenuParts.Label("Row " + (i + 1), panel, font, 32, TextAnchor.MiddleLeft, MenuParts.Plain, new Vector2(48f, ListTop - i * RowHeight), new Vector2(530f, RowHeight - 6f)));
            // Under the most rows there can be (the fifth ends at -374), with room for three lines above the hint.
            note = MenuParts.Label("Note", panel, font, 24, TextAnchor.UpperLeft, MenuParts.Faint, new Vector2(44f, -392f), new Vector2(540f, 110f));
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            hint = MenuParts.Label("Hint", panel, font, 21, TextAnchor.LowerLeft, MenuParts.Faint, new Vector2(44f, -526f), new Vector2(540f, 28f));

            gameObject.SetActive(false);
            transform.SetAsLastSibling();
        }

        /// <summary>Opens the menu at a fire: the world stops until it is left.</summary>
        public void Open(Component fire)
        {
            if (player == null) return;
            Fire = fire;
            Choices = new RestChoices();
            Choices.Reset();
            Opened++;
            if (!IsOpen)
            {
                savedScale = Time.timeScale > 0f ? Time.timeScale : 1f;
                savedSuspended = false;
            }
            IsOpen = true;
            waiting = RestEntry.None;
            releasePlayer = false;
            Show();
        }

        // In front again: on opening, and when the Hand or the chart opened from here closes.
        private void Show()
        {
            openedOnFrame = Time.frameCount;
            heldDirection = 0;
            gameObject.SetActive(true);
            // Opened again in the frame between leaving and the release: the look was hidden, not the object.
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
            GetComponent<Image>().enabled = true;
            transform.SetAsLastSibling();
            Hold();
            hintVersion = -1;
            Draw();
        }

        private void Hold()
        {
            player.Suspended = true;
            Time.timeScale = 0f;
        }

        /// <summary>Leaves the fire: the world goes on. The player gets the buttons back on the next frame, so the press that left is not also a dodge.</summary>
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            waiting = RestEntry.None;
            // The object stays active, only its look hidden, so Update runs next frame to give the player back the buttons;
            // an inactive object never would, and the player would stay frozen at the fire.
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);
            GetComponent<Image>().enabled = false;
            Time.timeScale = savedScale;
            releasePlayer = true;
        }

        /// <summary>Does what an entry does, as the use button on it would.</summary>
        public bool Act(RestEntry entry)
        {
            if (!IsOpen) return false;
            switch (entry)
            {
                case RestEntry.Hand:
                {
                    // The Loadout (ui_rest_fire_plan.md 5.1, U3 and U4) on the scene's Menus object (UiToolkitBuilder.PlaceMenus).
                    var loadout = Toolkit.LoadoutScreen.Ensure();
                    if (loadout == null || !loadout.Open()) return false;
                    Leave(RestEntry.Hand);
                    return true;
                }
                case RestEntry.Chart:
                {
                    var chart = UpgradeMenu.Instance;
                    if (chart == null) return false;
                    Leave(RestEntry.Chart);
                    chart.Open();
                    return chart.IsOpen;
                }
                case RestEntry.Leave:
                    Close();
                    return true;
                default:
                    return false;
            }
        }

        // Steps aside for a screen opened from here; the world stays stopped under it.
        private void Leave(RestEntry to)
        {
            waiting = to;
            gameObject.SetActive(true);
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);
            GetComponent<Image>().enabled = false;
        }

        private void Return()
        {
            waiting = RestEntry.None;
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
            GetComponent<Image>().enabled = true;
            Show();
        }

        private void Draw()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                bool used = i < Choices.Count;
                rows[i].enabled = used;
                if (!used) continue;
                rows[i].text = RestChoices.Label(Choices.At(i));
                rows[i].color = i == Choices.Selected ? MenuParts.Gold : MenuParts.Plain;
            }
            cursor.anchoredPosition = new Vector2(26f, ListTop - Choices.Selected * RowHeight);
            note.text = RestChoices.Note(Choices.Current);
            ShowHint();
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            string choose = ControlDevices.Current == ControlDevice.Keys ? "W S or the wheel" : "Stick or d-pad";
            hint.text = $"{choose}: choose     {ControlNames.Of("Interact")}: go     {ControlNames.Of("Dodge")}: leave";
        }

        private void Update()
        {
            if (!IsOpen)
            {
                if (releasePlayer)
                {
                    releasePlayer = false;
                    if (player != null) player.Suspended = savedSuspended;
                    for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
                    GetComponent<Image>().enabled = true;
                    gameObject.SetActive(false);
                }
                return;
            }
            if (waiting != RestEntry.None)
            {
                // Back in front once the screen opened from here has closed (and on the frame after, so its last press is its own).
                bool stillOpen = waiting == RestEntry.Chart ? UpgradeMenu.Instance != null && UpgradeMenu.Instance.IsOpen
                    : Toolkit.LoadoutScreen.Showing || ShapeMenu.Showing;
                if (!stillOpen && !GuidePage.BusyThisFrame && !Toolkit.LoadoutScreen.BusyThisFrame && !ShapeMenu.BusyThisFrame) Return();
                return;
            }
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            // Words being read over the menu (a fire whose land speaks as it is lit) have the buttons first.
            if (GuidePage.BusyThisFrame || ReadingBox.BusyThisFrame) return;
            if (hintVersion != ControlDevices.Version) ShowHint();
            if (dodgeAction.WasPressedThisFrame() || (pauseAction != null && pauseAction.WasPressedThisFrame()) || (alwaysBack != null && alwaysBack.WasPressedThisFrame()))
            {
                Close();
                return;
            }
            if (interactAction.WasPressedThisFrame())
            {
                Act(Choices.Press());
                return;
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel != 0f) Move(wheel > 0f ? -1 : 1);
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    Vector2 at = mouse.position.ReadValue();
                    for (int i = 0; i < Choices.Count && i < rows.Count; i++)
                    {
                        if (!MenuParts.Contains(rows[i].rectTransform, at)) continue;
                        Choices.Choose(i);
                        Draw();
                        Act(Choices.Press());
                        return;
                    }
                }
            }
            float stick = moveAction.ReadValue<Vector2>().y;
            int direction = stick > 0.5f ? -1 : stick < -0.5f ? 1 : 0;
            if (direction != heldDirection)
            {
                heldDirection = direction;
                untilRepeat = RepeatSeconds * 1.6f;
                if (direction != 0) Move(direction);
            }
            else if (direction != 0)
            {
                untilRepeat -= Time.unscaledDeltaTime;
                if (untilRepeat > 0f) return;
                untilRepeat = RepeatSeconds;
                Move(direction);
            }
        }

        // The chart lets the player go on the frame after it closes; while this menu is open the world stays held.
        private void LateUpdate()
        {
            if (IsOpen && player != null && (!player.Suspended || Time.timeScale != 0f)) Hold();
        }

        public void Move(int by)
        {
            Choices.Move(by);
            Draw();
        }
    }
}
