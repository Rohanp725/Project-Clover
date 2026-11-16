using System.Collections.Generic;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The element's tree, as shown at a rest fire: what can be learned, what each thing costs in embers and what it
    /// does. The world stops while it is open and the player reads at their own pace; the stick chooses, the use
    /// button learns, the dodge button leaves.
    /// </summary>
    public class UpgradeMenu : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerUpgrades upgrades;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title;
        [SerializeField] private Text description;
        [SerializeField] private Text[] rows = new Text[0];
        [SerializeField] private Text[] costs = new Text[0];
        [SerializeField] private RectTransform cursor;

        public Color Learned = new Color(1f, 0.78f, 0.35f, 1f);
        public Color Affordable = new Color(1f, 0.97f, 0.9f, 1f);
        public Color TooDear = new Color(0.6f, 0.58f, 0.6f, 1f);
        /// <summary>Real seconds the stick must be held before the choice moves again.</summary>
        public float RepeatSeconds = 0.22f;

        public static UpgradeMenu Instance { get; private set; }
        public bool IsOpen { get; private set; }
        public int Selected { get; private set; }

        private readonly List<Upgrade> shown = new List<Upgrade>();
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        private int openedOnFrame;
        private bool releasePlayer;
        private float untilRepeat;
        private int heldDirection;
        private float scaleBefore = 1f;

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public void Configure(PlayerController playerToServe, GameObject panelRoot, Text titleText, Text descriptionText, Text[] rowTexts, Text[] costTexts, RectTransform cursorBar)
        {
            player = playerToServe;
            upgrades = playerToServe != null ? playerToServe.GetComponent<PlayerUpgrades>() : null;
            panel = panelRoot;
            title = titleText;
            description = descriptionText;
            rows = rowTexts;
            costs = costTexts;
            cursor = cursorBar;
        }

        private void OnEnable() => Instance = this;

        private void OnDisable()
        {
            if (IsOpen) Close();
            if (releasePlayer && player != null) player.Suspended = false;
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
            if (player == null) return;
            moveAction = player.FindAction("Move");
            interactAction = player.FindAction("Interact");
            dodgeAction = player.FindAction("Dodge");
        }

        /// <summary>Opens the tree of the element the player holds. The world stops until it is closed.</summary>
        public void Open()
        {
            if (IsOpen || player == null || upgrades == null || panel == null) return;
            shown.Clear();
            foreach (var upgrade in Upgrade.All)
                if (upgrade.Element == upgrades.ElementName && !upgrade.Signature && shown.Count < rows.Length) shown.Add(upgrade);
            IsOpen = true;
            releasePlayer = false;
            openedOnFrame = Time.frameCount;
            Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, shown.Count - 1));
            heldDirection = 0;
            panel.SetActive(true);
            player.Suspended = true;
            // Whatever pace time had is given back on leaving, unless it was already stopped.
            scaleBefore = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            Show();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (panel != null) panel.SetActive(false);
            // Back to plain time, not to whatever pace a blow's slow had it at when the fire was used: that slow has let go by now.
            if (Time.timeScale == 0f) Time.timeScale = 1f;
            // The player gets the buttons back on the next frame, so the press that closed this is not also a dodge.
            releasePlayer = true;
        }

        /// <summary>Learns the chosen upgrade if the embers are there. Returns true when it was learned.</summary>
        public bool LearnSelected()
        {
            if (!IsOpen || Selected >= shown.Count) return false;
            bool learned = upgrades.TryBuy(shown[Selected]);
            Show();
            return learned;
        }

        /// <summary>Moves the choice up or down the list.</summary>
        public void Move(int by)
        {
            if (!IsOpen || shown.Count == 0) return;
            Selected = (Selected + by + shown.Count) % shown.Count;
            Show();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                if (releasePlayer)
                {
                    releasePlayer = false;
                    if (player != null) player.Suspended = false;
                }
                return;
            }
            // The press that opened it belongs to the rest fire, not to the first thing on the list.
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;

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

            if (interactAction.WasPressedThisFrame()) LearnSelected();
            else if (dodgeAction.WasPressedThisFrame()) Close();
        }

        private void Show()
        {
            if (title != null) title.text = $"{upgrades.ElementName}      Embers  {upgrades.Points}";
            for (int i = 0; i < rows.Length; i++)
            {
                bool used = i < shown.Count;
                if (rows[i] != null) rows[i].enabled = used;
                if (i < costs.Length && costs[i] != null) costs[i].enabled = used;
                if (!used) continue;
                var upgrade = shown[i];
                bool owned = upgrades.Owns(upgrade.Id);
                var colour = owned ? Learned : upgrades.Points >= upgrade.Cost ? Affordable : TooDear;
                if (rows[i] != null)
                {
                    rows[i].text = upgrade.Name;
                    rows[i].color = colour;
                }
                if (i < costs.Length && costs[i] != null)
                {
                    costs[i].text = owned ? "learned" : upgrade.Cost.ToString();
                    costs[i].color = colour;
                }
            }
            if (Selected < shown.Count)
            {
                var chosen = shown[Selected];
                if (description != null)
                {
                    string needs = string.IsNullOrEmpty(chosen.Weapon) ? "" : $"\nNeeds the {chosen.Weapon}.";
                    description.text = chosen.Description + needs;
                }
                if (cursor != null && rows[Selected] != null)
                    cursor.anchoredPosition = new Vector2(cursor.anchoredPosition.x, rows[Selected].rectTransform.anchoredPosition.y);
            }
        }
    }
}
