using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The pause menu: the world stops, and the player may go on, change the controls, give up the run, forget
    /// everything or quit. The stick (or the d-pad, or the mouse) chooses, the use button (or a click) picks, the dodge
    /// button goes back, and the pause button closes it from anywhere. Escape and Start always open and close it as well,
    /// whatever the pause action has been rebound to, so no binding can lock the player out of the menu. It will not
    /// open over the other menus, over spoken words, or once the player has fallen.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject list;
        [SerializeField] private GameObject controls;
        [SerializeField] private Text[] rows = new Text[0];
        [SerializeField] private Text note;
        [SerializeField] private RectTransform cursor;

        public Color Plain = new Color(1f, 0.97f, 0.9f, 1f);
        public Color Asked = new Color(1f, 0.42f, 0.3f, 1f);
        /// <summary>Real seconds the stick must be held before the choice moves again.</summary>
        public float RepeatSeconds = 0.22f;

        public static PauseMenu Instance { get; private set; }
        public bool IsOpen { get; private set; }
        public bool ShowingControls { get; private set; }
        public PauseChoices Choices { get; private set; }
        /// <summary>How many times quitting was confirmed; in the editor nothing else comes of it.</summary>
        public int Quits { get; private set; }

        private InputAction pauseAction;
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        // Escape and Start, outside the player's rebindable controls: the way back to this menu can never be lost.
        private InputAction alwaysPause;
        private ControlsPage controlsPage;
        private Text hint;
        private int hintVersion = -1;
        private int openedOnFrame;
        private bool releasePlayer;
        private bool stoppedTime;
        private bool leaving;
        private float untilRepeat;
        private int heldDirection;

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public void Configure(PlayerController playerToServe, GameObject panelRoot, GameObject listRoot, GameObject controlsRoot, Text[] rowTexts, Text noteText, RectTransform cursorBar)
        {
            player = playerToServe;
            defence = playerToServe != null ? playerToServe.GetComponent<PlayerDefence>() : null;
            panel = panelRoot;
            list = listRoot;
            controls = controlsRoot;
            rows = rowTexts;
            note = noteText;
            cursor = cursorBar;
        }

        private void OnEnable() => Instance = this;

        private void OnDisable()
        {
            if (IsOpen) Close();
            if (releasePlayer && player != null) player.Suspended = false;
            releasePlayer = false;
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            Choices = new PauseChoices(SceneManager.GetActiveScene().name);
            // The playtest offers more entries than the scene's builder laid rows for: the rest are made like the last.
            if (rows.Length > 0 && rows[rows.Length - 1] != null && rows.Length < Choices.Count)
            {
                var more = new Text[Choices.Count];
                System.Array.Copy(rows, more, rows.Length);
                var last = rows[rows.Length - 1];
                float step = rows.Length > 1 && rows[rows.Length - 2] != null
                    ? last.rectTransform.anchoredPosition.y - rows[rows.Length - 2].rectTransform.anchoredPosition.y : -68f;
                for (int i = rows.Length; i < more.Length; i++)
                {
                    more[i] = Instantiate(last, last.transform.parent);
                    more[i].name = "Row " + (i + 1);
                    more[i].rectTransform.anchoredPosition = last.rectTransform.anchoredPosition + new Vector2(0f, step * (i - rows.Length + 1));
                }
                rows = more;
            }
            if (panel != null) panel.SetActive(false);
            // The lines on the HUD that name buttons follow the device in use.
            PromptLine.Attach(transform);
            if (player == null) return;
            pauseAction = player.FindAction("Pause");
            moveAction = player.FindAction("Move");
            interactAction = player.FindAction("Interact");
            dodgeAction = player.FindAction("Dodge");
            alwaysPause = new InputAction("Always Pause", InputActionType.Button);
            foreach (string path in ControlBindings.Reserved) alwaysPause.AddBinding(path);
            alwaysPause.Enable();
            if (panel != null)
            {
                var hintObject = panel.transform.Find("Panel/Hint");
                hint = hintObject != null ? hintObject.GetComponent<Text>() : null;
            }
            if (controls != null)
            {
                controlsPage = controls.GetComponent<ControlsPage>();
                if (controlsPage == null) controlsPage = controls.AddComponent<ControlsPage>();
                var cursorImage = cursor != null ? cursor.GetComponent<Image>() : null;
                controlsPage.Setup(player, note, cursorImage != null ? cursorImage.sprite : null);
            }
        }

        private void OnDestroy()
        {
            if (alwaysPause == null) return;
            alwaysPause.Disable();
            alwaysPause.Dispose();
            alwaysPause = null;
        }

        /// <summary>The page that lists and rebinds the controls; null before the menu has started.</summary>
        public ControlsPage ControlsPage => controlsPage;

        /// <summary>True when nothing else has the screen or the player: no other menu, no spoken words, no fall, no leaving.</summary>
        public bool CanOpen =>
            !IsOpen && !leaving && player != null && panel != null && Choices != null
            // Anything that holds the player still (the fire's menu, words being read) has the buttons already.
            && !player.Suspended
            && (UpgradeMenu.Instance == null || !UpgradeMenu.Instance.IsOpen)
            && (ReadingBox.Instance == null || !ReadingBox.Instance.IsOpen)
            && !GuidePage.Showing
            // A fallen player is on the way to the dark; there is nothing left to pause.
            && (defence == null || !defence.IsDead);

        public void Open()
        {
            if (!CanOpen) return;
            IsOpen = true;
            releasePlayer = false;
            openedOnFrame = Time.frameCount;
            heldDirection = 0;
            Choices.Reset();
            ShowingControls = false;
            panel.SetActive(true);
            player.Suspended = true;
            // Only time this menu stopped is started again on leaving. A blow's brief slow lets go by itself when it
            // sees the scale was changed, so there is no slowed pace worth keeping.
            stoppedTime = Time.timeScale > 0f;
            Time.timeScale = 0f;
            Show();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (panel != null) panel.SetActive(false);
            if (stoppedTime && Time.timeScale == 0f) Time.timeScale = 1f;
            stoppedTime = false;
            // The player gets the buttons back on the next frame, so the press that closed this is not also a dodge.
            releasePlayer = true;
        }

        /// <summary>Moves the choice up or down the list.</summary>
        public void Move(int by)
        {
            if (!IsOpen || ShowingControls) return;
            Choices.Move(by);
            Show();
        }

        /// <summary>The use button: picks the chosen entry, or asks first when it cannot be taken back.</summary>
        public void Pick()
        {
            if (!IsOpen) return;
            if (ShowingControls)
            {
                if (controlsPage != null) controlsPage.Pick();
                else Back();
                return;
            }
            switch (Choices.Press())
            {
                case PauseEntry.Resume:
                    Close();
                    break;
                case PauseEntry.Controls:
                    ShowingControls = true;
                    if (controlsPage != null) controlsPage.Opened();
                    Show();
                    break;
                case PauseEntry.Fullscreen:
                    ScreenFit.ToggleFullscreen();
                    Show();
                    break;
                case PauseEntry.GiveUp:
                    // As a fall ends it, without the falling.
                    RunState.End(true);
                    Leave();
                    break;
                case PauseEntry.Guide:
                    var guide = GuidePage.Ensure();
                    if (guide != null) guide.OpenGuide();
                    break;
                case PauseEntry.WhatsNew:
                    var page = GuidePage.Ensure();
                    if (page != null) page.OpenWhatsNew();
                    break;
                case PauseEntry.ProvingGround:
                    // Everything carried goes with the player: the playtest loses nothing on the way home.
                    Leave(PlaytestMode.ProvingGroundScene);
                    break;
                case PauseEntry.TrialOutfit:
                    Outfits.Chosen = Outfits.ToggleTrial(Outfits.Chosen);
                    // Changed on the spot, so the two bodies can be compared without leaving the scene.
                    var outfit = player.GetComponent<PlayerOutfit>();
                    if (outfit != null) outfit.Dress();
                    Show();
                    break;
                case PauseEntry.Forget:
                    Progress.Forget(Progress.MetSponsor);
                    Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
                    Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
                    // What was taken from a stand that is now sealed again is put down too.
                    RunState.Reset();
                    Leave();
                    break;
                case PauseEntry.Quit:
                    Quits++;
                    Close();
                    // The editor ignores this, and the menu has simply closed.
                    Application.Quit();
                    break;
                default:
                    Show();
                    break;
            }
        }

        /// <summary>The dodge button: out of the controls, out of a question, or out of the menu.</summary>
        public void Back()
        {
            if (!IsOpen) return;
            if (ShowingControls && controlsPage != null && controlsPage.Busy) return;
            if (ShowingControls)
            {
                ShowingControls = false;
                Show();
            }
            else if (Choices.Cancel()) Show();
            else Close();
        }

        // The roads out of the scene lead to the hub (in the playtest, to the Proving Ground), with time running and no
        // menu able to open on the way.
        private void Leave(string scene = PauseChoices.HubScene)
        {
            leaving = true;
            Close();
            Time.timeScale = 1f;
            SceneRoutes.Load(scene);
        }

        private void Update()
        {
            // The guide's page lies over this menu and has the buttons while it is open, and on the frame it closes.
            if (GuidePage.BusyThisFrame) return;
            bool pausePressed = (pauseAction != null && pauseAction.WasPressedThisFrame()) || (alwaysPause != null && alwaysPause.WasPressedThisFrame());
            if (!IsOpen)
            {
                if (releasePlayer)
                {
                    releasePlayer = false;
                    if (player != null) player.Suspended = false;
                    return;
                }
                if (pausePressed) Open();
                return;
            }
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            // A rebind in progress has every button; Escape and Start then mean "leave it as it was", not "resume".
            if (ShowingControls && controlsPage != null && controlsPage.Busy)
            {
                controlsPage.Tick(Vector2.zero, false, false);
                ShowHint();
                return;
            }
            if (pausePressed)
            {
                Close();
                return;
            }
            if (hintVersion != ControlDevices.Version) ShowHint();

            if (ShowingControls && controlsPage != null)
            {
                if (controlsPage.Tick(moveAction.ReadValue<Vector2>(), interactAction.WasPressedThisFrame(), dodgeAction.WasPressedThisFrame())) Back();
                ShowHint();
                return;
            }

            if (!ShowingControls)
            {
                PointAt();
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
                    if (untilRepeat <= 0f)
                    {
                        untilRepeat = RepeatSeconds;
                        Move(direction);
                    }
                }
            }

            if (interactAction.WasPressedThisFrame()) Pick();
            else if (dodgeAction.WasPressedThisFrame()) Back();
        }

        // The mouse: the entry under the cursor is chosen as it moves over it, and a click picks it.
        private void PointAt()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            bool clicked = mouse.leftButton.wasPressedThisFrame;
            if (!clicked && mouse.delta.ReadValue().sqrMagnitude <= 1f) return;
            Vector2 at = mouse.position.ReadValue();
            for (int i = 0; i < rows.Length && i < Choices.Count; i++)
            {
                if (rows[i] == null || !RectTransformUtility.RectangleContainsScreenPoint(rows[i].rectTransform, at, null)) continue;
                if (i != Choices.Selected) Move(i - Choices.Selected);
                if (clicked) Pick();
                return;
            }
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            if (hint == null) return;
            if (ShowingControls && controlsPage != null)
            {
                hint.text = controlsPage.Hint();
                return;
            }
            var style = ControlDevices.Current;
            string choose = style == ControlDevice.Keys ? "W S or the mouse" : "Stick or d-pad";
            hint.text = $"{choose}: choose     {ControlNames.Of("Interact")}: pick     {ControlNames.Of("Dodge")}: back     "
                + $"{(style == ControlDevice.Keys ? "Esc" : ControlNames.Pad("<Gamepad>/start", style))}: resume";
        }

        // Called when something changes, never every frame.
        private void Show()
        {
            ShowHint();
            if (list != null) list.SetActive(!ShowingControls);
            if (controls != null) controls.SetActive(ShowingControls);
            if (ShowingControls) return;
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null) continue;
                bool used = i < Choices.Count;
                rows[i].enabled = used;
                if (!used) continue;
                bool asking = Choices.Asking && i == Choices.Selected;
                rows[i].text = asking ? PauseChoices.Question(Choices.At(i)) : PauseChoices.Label(Choices.At(i));
                rows[i].color = asking ? Asked : Plain;
            }
            if (note != null) note.text = PauseChoices.Note(Choices.Current);
            if (cursor != null && Choices.Selected < rows.Length && rows[Choices.Selected] != null)
                cursor.anchoredPosition = new Vector2(cursor.anchoredPosition.x, rows[Choices.Selected].rectTransform.anchoredPosition.y);
        }
    }
}
