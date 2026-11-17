using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The pause menu: the world stops, and the player may go on, read the controls, give up the run, forget
    /// everything or quit. The stick chooses, the use button picks, the dodge button goes back, and the pause button
    /// closes it from anywhere. It will not open over the other menus, over spoken words, or once the player has fallen.
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
            if (panel != null) panel.SetActive(false);
            if (player == null) return;
            pauseAction = player.FindAction("Pause");
            moveAction = player.FindAction("Move");
            interactAction = player.FindAction("Interact");
            dodgeAction = player.FindAction("Dodge");
        }

        /// <summary>True when nothing else has the screen or the player: no other menu, no spoken words, no fall, no leaving.</summary>
        public bool CanOpen =>
            !IsOpen && !leaving && player != null && panel != null && Choices != null
            // Anything that holds the player still (the fire's menu, words being read) has the buttons already.
            && !player.Suspended
            && (UpgradeMenu.Instance == null || !UpgradeMenu.Instance.IsOpen)
            && (ReadingBox.Instance == null || !ReadingBox.Instance.IsOpen)
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
                Back();
                return;
            }
            switch (Choices.Press())
            {
                case PauseEntry.Resume:
                    Close();
                    break;
                case PauseEntry.Controls:
                    ShowingControls = true;
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
            if (ShowingControls)
            {
                ShowingControls = false;
                Show();
            }
            else if (Choices.Cancel()) Show();
            else Close();
        }

        // Both roads out of the scene lead to the hub, with time running and no menu able to open on the way.
        private void Leave()
        {
            leaving = true;
            Close();
            Time.timeScale = 1f;
            SceneManager.LoadScene(PauseChoices.HubScene);
        }

        private void Update()
        {
            if (!IsOpen)
            {
                if (releasePlayer)
                {
                    releasePlayer = false;
                    if (player != null) player.Suspended = false;
                    return;
                }
                if (pauseAction != null && pauseAction.WasPressedThisFrame()) Open();
                return;
            }
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            if (pauseAction.WasPressedThisFrame())
            {
                Close();
                return;
            }

            if (!ShowingControls)
            {
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

        // Called when something changes, never every frame.
        private void Show()
        {
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
