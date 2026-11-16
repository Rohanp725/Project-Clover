using System.Collections.Generic;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The chart of the element trees, as shown at a rest fire. Every element's tree grows outward from one middle;
    /// the view stays close on the tree of the element the player holds, with its neighbours faint at the edges.
    /// Each thing to learn is a mark joined by a line to what it grows from, and a branch is learned from its foot
    /// outward. The world stops while it is open and the player reads at their own pace; the stick moves from mark
    /// to mark, the use button learns, the dodge button leaves.
    /// </summary>
    public class UpgradeMenu : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerUpgrades upgrades;
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform chart;
        [SerializeField] private Text title;
        [SerializeField] private Text chosenName;
        [SerializeField] private Text chosenState;
        [SerializeField] private Text description;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private Sprite markSprite;
        [SerializeField] private Sprite emblemSprite;

        public Color FireColour = new Color(1f, 0.56f, 0.2f, 1f);
        public Color LightningColour = new Color(0.45f, 0.78f, 1f, 1f);
        public Color Affordable = new Color(1f, 0.97f, 0.9f, 1f);
        public Color TooDear = new Color(0.62f, 0.6f, 0.7f, 1f);
        public Color OutOfReach = new Color(0.3f, 0.29f, 0.38f, 1f);
        /// <summary>How strongly the trees of elements the player does not hold are drawn.</summary>
        public float OtherTrees = 0.3f;
        /// <summary>Pixels to one step of the chart, at the reference screen.</summary>
        public float StepPixels = 124f;
        public float MarkSize = 78f;
        /// <summary>Where on the screen the held tree is kept, away from the words on the right.</summary>
        public Vector2 ViewCentre = new Vector2(-290f, -30f);
        /// <summary>How far the view leans from the middle of the tree toward the chosen mark: 0 holds still, 1 follows it.</summary>
        public float FollowChoice = 0.45f;
        /// <summary>Real seconds the stick must be held before the choice moves again.</summary>
        public float RepeatSeconds = 0.24f;

        public static UpgradeMenu Instance { get; private set; }
        public bool IsOpen { get; private set; }
        /// <summary>Which of the held element's marks is chosen, counting in the order of <see cref="Upgrade.All"/>.</summary>
        public int Selected { get; private set; }
        public Upgrade Chosen => Selected < shown.Count ? marks[shown[Selected]].Upgrade : null;

        private class Mark
        {
            public Upgrade Upgrade;
            public Vector2 Place;
            public Image Frame;
            public Text Letter;
            public Text Cost;
            public Image Line;
        }

        private readonly List<Mark> marks = new List<Mark>();
        private readonly List<Image> emblems = new List<Image>();
        private readonly List<int> shown = new List<int>();
        private readonly List<Vector2> places = new List<Vector2>();
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        private int openedOnFrame;
        private bool releasePlayer;
        private float untilRepeat;
        private bool stickHeld;
        private bool built;
        private Vector2 treeMiddle;

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public void Configure(PlayerController playerToServe, GameObject panelRoot, RectTransform chartRoot, Text titleText, Text nameText, Text stateText,
            Text descriptionText, RectTransform cursorFrame, Sprite mark, Sprite emblem)
        {
            player = playerToServe;
            upgrades = playerToServe != null ? playerToServe.GetComponent<PlayerUpgrades>() : null;
            panel = panelRoot;
            chart = chartRoot;
            title = titleText;
            chosenName = nameText;
            chosenState = stateText;
            description = descriptionText;
            cursor = cursorFrame;
            markSprite = mark;
            emblemSprite = emblem;
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

        private Color ColourOf(string element) => element == ElementKit.LightningName ? LightningColour : FireColour;

        // The chart is drawn from the catalogue the first time it is opened, so a new upgrade needs no new scene.
        private void Build()
        {
            built = true;
            if (chart == null) return;
            var byId = new Dictionary<string, Mark>();
            foreach (var upgrade in Upgrade.All)
            {
                var mark = new Mark { Upgrade = upgrade, Place = UpgradeTree.OnChart(upgrade) * StepPixels };
                marks.Add(mark);
                byId[upgrade.Id] = mark;
            }
            // Lines first, so every mark lies over the lines that reach it.
            foreach (var mark in marks)
            {
                if (mark.Upgrade.Signature) continue;
                Vector2 from = byId.TryGetValue(mark.Upgrade.Requires, out var parent) ? parent.Place : UpgradeTree.Emblem(mark.Upgrade.Element) * StepPixels;
                mark.Line = Line(from, mark.Place);
            }
            foreach (string element in UpgradeTree.Elements)
            {
                var emblem = Picture("Emblem " + element, emblemSprite, UpgradeTree.Emblem(element) * StepPixels, MarkSize * 0.8f);
                emblem.color = ColourOf(element);
                emblems.Add(emblem);
            }
            foreach (var mark in marks)
            {
                float size = mark.Upgrade.Signature ? MarkSize * 1.25f : MarkSize;
                mark.Frame = Picture(mark.Upgrade.Name, markSprite, mark.Place, size);
                mark.Frame.type = Image.Type.Sliced;
                mark.Letter = Words(mark.Frame.transform, mark.Upgrade.Name.Substring(0, 1), Mathf.RoundToInt(size * 0.56f), Vector2.zero);
                mark.Cost = Words(mark.Frame.transform, "", 22, new Vector2(0f, -size * 0.5f - 16f));
            }
            if (cursor != null) cursor.SetAsLastSibling();
        }

        private Image Picture(string name, Sprite sprite, Vector2 at, float size)
        {
            var made = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)made.transform;
            rect.SetParent(chart, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(size, size);
            var image = made.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private Image Line(Vector2 from, Vector2 to)
        {
            var image = Picture("Line", null, from, 5f);
            var rect = image.rectTransform;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2((to - from).magnitude, 5f);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
            return image;
        }

        private Text Words(Transform parent, string text, int size, Vector2 at)
        {
            var made = new GameObject("Words", typeof(RectTransform));
            var rect = (RectTransform)made.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(120f, 40f);
            var words = made.AddComponent<Text>();
            words.font = title != null ? title.font : null;
            words.fontSize = size;
            words.alignment = TextAnchor.MiddleCenter;
            words.horizontalOverflow = HorizontalWrapMode.Overflow;
            words.verticalOverflow = VerticalWrapMode.Overflow;
            words.raycastTarget = false;
            words.text = text;
            return words;
        }

        /// <summary>Opens the chart on the tree of the element the player holds. The world stops until it is closed.</summary>
        public void Open()
        {
            if (IsOpen || player == null || upgrades == null || panel == null) return;
            if (!built) Build();
            shown.Clear();
            places.Clear();
            treeMiddle = Vector2.zero;
            for (int i = 0; i < marks.Count; i++)
            {
                if (marks[i].Upgrade.Element != upgrades.ElementName) continue;
                shown.Add(i);
                places.Add(marks[i].Place);
                treeMiddle += marks[i].Place;
            }
            if (shown.Count > 0) treeMiddle /= shown.Count;
            IsOpen = true;
            releasePlayer = false;
            openedOnFrame = Time.frameCount;
            Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, shown.Count - 1));
            stickHeld = false;
            panel.SetActive(true);
            player.Suspended = true;
            Time.timeScale = 0f;
            Show();
            if (chart != null) chart.anchoredPosition = ViewTarget();
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

        /// <summary>Learns the chosen upgrade if it can be reached and the embers are there. Returns true when it was learned.</summary>
        public bool LearnSelected()
        {
            if (!IsOpen || Chosen == null) return false;
            bool learned = upgrades.TryBuy(Chosen);
            Show();
            return learned;
        }

        /// <summary>Moves the choice to the mark that lies the given way on the chart, if one does. Returns true when it moved.</summary>
        public bool Step(Vector2 direction)
        {
            if (!IsOpen || shown.Count == 0) return false;
            int next = UpgradeTree.Step(places, Selected, direction);
            if (next == Selected) return false;
            Selected = next;
            Show();
            return true;
        }

        /// <summary>Puts the choice on the named upgrade, if it is on the tree shown. Returns true when it is.</summary>
        public bool Choose(string id)
        {
            if (!IsOpen) return false;
            for (int i = 0; i < shown.Count; i++)
            {
                if (marks[shown[i]].Upgrade.Id != id) continue;
                Selected = i;
                Show();
                return true;
            }
            return false;
        }

        private Vector2 ViewTarget()
        {
            Vector2 chosen = Selected < places.Count ? places[Selected] : treeMiddle;
            return ViewCentre - Vector2.Lerp(treeMiddle, chosen, FollowChoice);
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
            float step = Time.unscaledDeltaTime;
            if (chart != null) chart.anchoredPosition = Vector2.Lerp(chart.anchoredPosition, ViewTarget(), 1f - Mathf.Exp(-9f * step));
            if (cursor != null)
            {
                float beat = 1f + 0.05f * Mathf.Sin(Time.unscaledTime * 5f);
                cursor.localScale = new Vector3(beat, beat, 1f);
            }
            // The press that opened it belongs to the rest fire, not to the first mark on the chart.
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;

            Vector2 stick = moveAction.ReadValue<Vector2>();
            bool pushed = stick.sqrMagnitude > 0.25f;
            if (pushed && !stickHeld)
            {
                untilRepeat = RepeatSeconds * 1.6f;
                Step(stick);
            }
            else if (pushed)
            {
                untilRepeat -= step;
                if (untilRepeat <= 0f)
                {
                    untilRepeat = RepeatSeconds;
                    Step(stick);
                }
            }
            stickHeld = pushed;

            if (interactAction.WasPressedThisFrame()) LearnSelected();
            else if (dodgeAction.WasPressedThisFrame()) Close();
        }

        private void Show()
        {
            string held = upgrades.ElementName;
            if (title != null) title.text = $"{held}      Embers  {upgrades.Points}";
            for (int i = 0; i < emblems.Count; i++)
            {
                string element = UpgradeTree.Elements[i];
                var colour = ColourOf(element);
                colour.a = element == held ? 1f : OtherTrees;
                emblems[i].color = colour;
            }
            foreach (var mark in marks)
            {
                var upgrade = mark.Upgrade;
                bool ours = upgrade.Element == held;
                bool owned = ours && upgrades.Owns(upgrade.Id);
                bool reachable = ours && !upgrade.Signature && UpgradeTree.Reachable(upgrade, Clover.Core.RunState.Unlocked);
                Color colour = owned ? ColourOf(upgrade.Element)
                    : !reachable ? OutOfReach
                    : upgrades.Points >= upgrade.Cost ? Affordable : TooDear;
                float strength = ours ? 1f : OtherTrees;
                if (!ours) colour = Color.Lerp(OutOfReach, ColourOf(upgrade.Element), 0.35f);
                colour.a = strength;
                mark.Frame.color = colour;
                mark.Letter.color = colour;
                mark.Cost.text = owned || upgrade.Signature || !ours ? "" : upgrade.Cost.ToString();
                mark.Cost.color = colour;
                if (mark.Line != null)
                {
                    // A line is lit once what stands at its far end is learned; it shows the way open when its near end is.
                    var line = owned ? ColourOf(upgrade.Element) : reachable ? TooDear : OutOfReach;
                    line.a = strength * (owned ? 0.95f : 0.7f);
                    mark.Line.color = line;
                }
            }
            var chosen = Chosen;
            if (chosen == null) return;
            if (cursor != null)
            {
                cursor.anchoredPosition = places[Selected];
                float size = (chosen.Signature ? MarkSize * 1.25f : MarkSize) + 22f;
                cursor.sizeDelta = new Vector2(size, size);
            }
            if (chosenName != null) chosenName.text = chosen.Name;
            if (chosenState != null)
            {
                bool owned = upgrades.Owns(chosen.Id);
                var parent = string.IsNullOrEmpty(chosen.Requires) ? null : Upgrade.ById(chosen.Requires);
                chosenState.text = chosen.Signature ? (owned ? "Left to you by a god" : "Only a god's fall gives this")
                    : owned ? "Learned"
                    : parent != null && !upgrades.Owns(parent.Id) ? $"{chosen.Cost} embers.  Grows from {parent.Name}"
                    : upgrades.Points >= chosen.Cost ? $"{chosen.Cost} embers" : $"{chosen.Cost} embers.  You carry {upgrades.Points}";
            }
            if (description != null)
            {
                string needs = string.IsNullOrEmpty(chosen.Weapon) ? "" : $"\nNeeds the {chosen.Weapon}.";
                description.text = chosen.Description + needs;
            }
        }
    }
}
