using System;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// Words the player reads at their own pace: one line at a time across the bottom of the screen, each staying until
    /// the use button is pressed (or the mouse clicked). The player stands still while it is open. A line may name a
    /// button as {ActionName}; it is written in the names of the device in use. Anything may call <see cref="Show"/>; with
    /// no box in the scene the call does nothing and what was to follow happens at once.
    /// </summary>
    public class ReadingBox : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text speaker;
        [SerializeField] private Text line;
        [SerializeField] private Text more;

        /// <summary>Real seconds a line takes to write itself out; a press before then shows it whole.</summary>
        public float WriteSeconds = 0.5f;

        public static ReadingBox Instance { get; private set; }
        public bool IsOpen { get; private set; }
        public int LinesRead { get; private set; }

        private string[] lines;
        private int index;
        private Action after;
        private InputAction interactAction;
        private int openedOnFrame;
        private bool releasePlayer;
        private float written;

        // Domain reload is off: the box of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public void Configure(PlayerController playerToHold, GameObject panelRoot, Text speakerText, Text lineText, Text moreText)
        {
            player = playerToHold;
            panel = panelRoot;
            speaker = speakerText;
            line = lineText;
            more = moreText;
        }

        private void OnEnable() => Instance = this;

        private void OnDisable()
        {
            if ((IsOpen || releasePlayer) && player != null) player.Suspended = false;
            IsOpen = false;
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (panel != null && !IsOpen) panel.SetActive(false);
            if (player != null) interactAction = player.FindAction("Interact");
        }

        /// <summary>Shows the lines one after another; when the last is read, does what follows (if anything).</summary>
        public static void Show(string who, string[] whatIsSaid, Action whenRead = null)
        {
            if (Instance == null || Instance.panel == null || whatIsSaid == null || whatIsSaid.Length == 0)
            {
                whenRead?.Invoke();
                return;
            }
            Instance.Open(who, whatIsSaid, whenRead);
        }

        private void Open(string who, string[] whatIsSaid, Action whenRead)
        {
            lines = new string[whatIsSaid.Length];
            for (int i = 0; i < lines.Length; i++) lines[i] = ControlNames.Expand(whatIsSaid[i]);
            after = whenRead;
            index = 0;
            IsOpen = true;
            releasePlayer = false;
            openedOnFrame = Time.frameCount;
            if (speaker != null) speaker.text = who;
            panel.SetActive(true);
            if (player != null) player.Suspended = true;
            Write();
        }

        private void Write()
        {
            written = 0f;
            if (line != null) line.text = "";
            if (more != null)
            {
                more.enabled = false;
                more.text = ControlNames.Of("Interact");
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                if (releasePlayer)
                {
                    // The player gets the buttons back a frame late, so the press that closed this is not also a use.
                    releasePlayer = false;
                    if (player != null) player.Suspended = false;
                }
                return;
            }
            if (interactAction == null && player != null) interactAction = player.FindAction("Interact");

            string whole = lines[index];
            bool done = written >= 1f;
            if (!done)
            {
                written = WriteSeconds > 0f ? Mathf.Min(1f, written + Time.unscaledDeltaTime / WriteSeconds) : 1f;
                int shown = Mathf.Clamp(Mathf.CeilToInt(whole.Length * written), 0, whole.Length);
                if (line != null) line.text = written >= 1f ? whole : whole.Substring(0, shown);
                if (written >= 1f && more != null) more.enabled = true;
            }

            var mouse = Mouse.current;
            bool clicked = mouse != null && mouse.leftButton.wasPressedThisFrame;
            if (Time.frameCount == openedOnFrame || interactAction == null || !(interactAction.WasPressedThisFrame() || clicked)) return;
            if (!done)
            {
                written = 1f;
                if (line != null) line.text = whole;
                if (more != null) more.enabled = true;
                return;
            }
            LinesRead++;
            index++;
            if (index < lines.Length)
            {
                Write();
                return;
            }
            IsOpen = false;
            panel.SetActive(false);
            releasePlayer = true;
            var then = after;
            after = null;
            then?.Invoke();
        }
    }
}
