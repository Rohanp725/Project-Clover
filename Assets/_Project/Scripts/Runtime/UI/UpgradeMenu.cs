using System.Collections.Generic;
using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The great chart, as shown at a rest fire: a night sky on which every element's tree grows outward from one
    /// middle, eleven of them round a ring, with the branch each shares with its neighbour in the gap between. The
    /// chart is turned so the tree of the element held stands upright, and the view stays close on it and follows the
    /// choice; the heavy-attack button pulls the view back to show the whole ring, and returns. Each mark is a framed
    /// picture joined by a thin line to what it grows from; its frame says what kind of mark it is. The world stops
    /// while the chart is open and the player reads at their own pace: the stick moves from mark to mark, the use
    /// button learns (or, on a spell already learned, wears it), the dodge button leaves.
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
        [SerializeField] private GameObject words;
        [SerializeField] private Text hint;
        [SerializeField] private RectTransform sky;
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField] private Sprite[] icons = new Sprite[0];
        [SerializeField] private Sprite emblemSprite;

        public Color Affordable = new Color(1f, 0.97f, 0.9f, 1f);
        public Color TooDear = new Color(0.6f, 0.6f, 0.72f, 1f);
        public Color OutOfReach = new Color(0.33f, 0.33f, 0.45f, 1f);
        public Color Sealed = new Color(0.5f, 0.3f, 0.36f, 1f);
        public Color GodsGift = new Color(0.62f, 0.52f, 0.3f, 1f);
        public Color Guide = new Color(0.55f, 0.6f, 0.85f, 0.16f);
        /// <summary>The frame of a playtest slot for what is not built yet.</summary>
        public Color NotBuiltColour = new Color(0.55f, 0.55f, 0.62f, 0.55f);
        /// <summary>How strongly the trees of elements the player does not hold are drawn.</summary>
        public float OtherTrees = 0.4f;
        public float WideTrees = 0.85f;
        /// <summary>Pixels to one step of the chart, at the reference screen, in the close view.</summary>
        public float StepPixels = 84f;
        /// <summary>How wide each kind of mark is, in steps, in the order of <see cref="UpgradeKind"/>.</summary>
        public float[] MarkSteps = { 0.8f, 0.5f, 0.68f, 0.78f, 0.94f, 0.7f, 1.15f };
        /// <summary>Where on the screen the held tree is kept in the close view, away from the words on the right, and where the ring's middle is kept in the wide one.</summary>
        public Vector2 ViewCentre = new Vector2(-320f, -20f);
        public Vector2 WideCentre = new Vector2(0f, -8f);
        /// <summary>How far the view leans from the middle of the tree toward the chosen mark: 0 holds still, 1 follows it.</summary>
        public float FollowChoice = 0.55f;
        /// <summary>Real seconds the stick must be held before the choice moves again.</summary>
        public float RepeatSeconds = 0.2f;
        /// <summary>How quickly the view moves and changes its distance.</summary>
        public float ViewSpeed = 9f;
        public float LabelScale = 0.42f;

        public static UpgradeMenu Instance { get; private set; }
        public bool IsOpen { get; private set; }
        /// <summary>True while the view is pulled back to show the whole ring.</summary>
        public bool IsWide { get; private set; }
        /// <summary>Which of the marks open to the choice is chosen: those of the held element's tree and of the two branches it shares.</summary>
        public int Selected { get; private set; }
        public Upgrade Chosen => Selected < shown.Count ? marks[shown[Selected]].Upgrade : null;
        /// <summary>The element whose tree the choice moves on: the one held, or in the playtest any tree walked to.</summary>
        public string Viewed => viewed;
        /// <summary>How many times the details view and the guide have been opened from the chart, for probes.</summary>
        public int DetailsOpened { get; private set; }
        public int GuideOpened { get; private set; }
        /// <summary>How many marks the chart holds, and how far back the view is now (1 is the close view).</summary>
        public int MarkCount => marks.Count;
        public float Zoom => zoom;

        private class Mark
        {
            public Upgrade Upgrade;
            public Vector2 Place;
            public float Size;
            public Image Frame;
            public Image Icon;
            public int FirstLine, LineCount;
            public Color Colour;
        }

        private readonly List<Mark> marks = new List<Mark>();
        private readonly List<Image> lines = new List<Image>();
        private readonly List<Image> emblems = new List<Image>();
        private readonly List<Image> emblemIcons = new List<Image>();
        private readonly List<Text> labels = new List<Text>();
        private readonly List<Text> mixLabels = new List<Text>();
        private readonly List<RectTransform> feet = new List<RectTransform>();
        private readonly List<RectTransform> upright = new List<RectTransform>();
        private readonly List<int> shown = new List<int>();
        private readonly List<Vector2> places = new List<Vector2>();
        private readonly Dictionary<string, Sprite> iconByName = new Dictionary<string, Sprite>();
        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction dodgeAction;
        private InputAction heavyAction;
        private InputAction spellAction;
        private InputAction flaskAction;
        private InputAction guardAction;
        private InputAction lightAction;
        private string viewed;
        private bool slotsDrawn;
        private int hintVersion = -1;
        private RectTransform wornBadge;
        private int openedOnFrame;
        private bool releasePlayer;
        private float untilRepeat;
        private bool stickHeld;
        private bool built;
        private float bearing;
        private float zoom = 1f;
        private float labelZoom = -1f;
        private Vector2 treeMiddle;
        private string heldShown;

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public void Configure(PlayerController playerToServe, GameObject panelRoot, RectTransform chartRoot, Text titleText, Text nameText, Text stateText,
            Text descriptionText, RectTransform cursorFrame, GameObject wordsPanel, Text hintText, RectTransform skyBehind, Sprite[] frameSprites, Sprite[] iconSprites, Sprite emblem)
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
            words = wordsPanel;
            hint = hintText;
            sky = skyBehind;
            frames = frameSprites;
            icons = iconSprites;
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
            heavyAction = player.FindAction("HeavyAttack");
            spellAction = player.FindAction("Spell");
            flaskAction = player.FindAction("Flask");
            guardAction = player.FindAction("Guard");
            lightAction = player.FindAction("LightAttack");
        }

        private static Color ColourOf(Upgrade upgrade) =>
            upgrade.IsMix ? Color.Lerp(Elements.Colour(upgrade.Element), Elements.Colour(upgrade.Element2), 0.5f) : Elements.Colour(upgrade.Element);

        private float SizeOf(Upgrade upgrade)
        {
            int kind = (int)upgrade.Kind;
            return (kind < MarkSteps.Length ? MarkSteps[kind] : 0.7f) * StepPixels;
        }

        /// <summary>Draws the chart again from the catalogue, as after a probe has added marks to it.</summary>
        public void Rebuild()
        {
            if (chart == null) return;
            for (int i = chart.childCount - 1; i >= 0; i--)
            {
                var child = chart.GetChild(i);
                if (child != cursor) Destroy(child.gameObject);
            }
            marks.Clear();
            lines.Clear();
            emblems.Clear();
            emblemIcons.Clear();
            labels.Clear();
            mixLabels.Clear();
            feet.Clear();
            upright.Clear();
            wornBadge = null;
            heldShown = null;
            Build();
        }

        // The chart is drawn from the catalogue the first time it is opened, so a new upgrade needs no new scene.
        private void Build()
        {
            built = true;
            if (chart == null) return;
            iconByName.Clear();
            for (int i = 0; i < icons.Length && i < UpgradeIcons.All.Count; i++) iconByName[UpgradeIcons.All[i]] = icons[i];

            // A faint ring through the feet of the trees, and a spoke to each: the lines of a star chart.
            const int Segments = 88;
            float ringRadius = UpgradeTree.EmblemDistance * StepPixels;
            for (int i = 0; i < Segments; i++)
            {
                float from = i * Mathf.PI * 2f / Segments, to = (i + 1) * Mathf.PI * 2f / Segments;
                Line(new Vector2(Mathf.Sin(from), Mathf.Cos(from)) * ringRadius, new Vector2(Mathf.Sin(to), Mathf.Cos(to)) * ringRadius, 5f).color = Guide;
            }
            foreach (var element in Elements.All)
            {
                Vector2 foot = UpgradeTree.Emblem(element.Name) * StepPixels;
                Line(foot * 0.12f, foot * 0.92f, 4f).color = Guide;
                // The branch it shares with the next element round: a line from each of the two to the branch's foot.
                Vector2 mixFoot = UpgradeTree.MixFoot(element.Name, element.MixWith) * StepPixels;
                Color shared = Color.Lerp(element.Colour, Elements.Colour(element.MixWith), 0.5f);
                shared.a = 0.3f;
                Line(foot, mixFoot, 3f).color = shared;
                Line(UpgradeTree.Emblem(element.MixWith) * StepPixels, mixFoot, 3f).color = shared;
            }

            var byId = new Dictionary<string, Mark>();
            foreach (var upgrade in Upgrade.All)
            {
                var mark = new Mark { Upgrade = upgrade, Place = UpgradeTree.OnChart(upgrade) * StepPixels, Size = SizeOf(upgrade), Colour = ColourOf(upgrade) };
                marks.Add(mark);
                byId[upgrade.Id] = mark;
            }
            // In the playtest, where each god's bough on the weapon tree will be: drawn as a god's gift, faint, never bought.
            slotsDrawn = PlaytestMode.On;
            if (slotsDrawn)
                foreach (var slot in NotBuiltYet.ChartSlots)
                    marks.Add(new Mark { Upgrade = slot, Place = UpgradeTree.OnChart(slot) * StepPixels, Size = SizeOf(slot) * 0.8f, Colour = NotBuiltColour });
            // Lines first, so every mark lies over the lines that reach it.
            foreach (var mark in marks)
            {
                mark.FirstLine = lines.Count;
                var upgrade = mark.Upgrade;
                if (upgrade.Signature) continue;
                bool any = false;
                foreach (string id in upgrade.Requires)
                {
                    if (!byId.TryGetValue(id, out var parent)) continue;
                    lines.Add(Line(parent.Place, mark.Place, 3f));
                    any = true;
                }
                if (!any)
                    lines.Add(Line((upgrade.IsMix ? UpgradeTree.MixFoot(upgrade.Element, upgrade.Element2) : UpgradeTree.Emblem(upgrade.Element)) * StepPixels, mark.Place, 3f));
                mark.LineCount = lines.Count - mark.FirstLine;
            }
            foreach (var element in Elements.All)
            {
                Vector2 foot = UpgradeTree.Emblem(element.Name) * StepPixels;
                var emblem = Picture("Emblem " + element.Name, emblemSprite, chart, foot, StepPixels * 1.05f);
                emblems.Add(emblem);
                upright.Add(emblem.rectTransform);
                feet.Add(emblem.rectTransform);
                iconByName.TryGetValue(element.Icon, out var picture);
                emblemIcons.Add(Picture("Picture", picture, emblem.transform, Vector2.zero, StepPixels * 0.5f));
                // Its name, on the inner side of its emblem; and the name of the branch it shares, at that branch's foot.
                // Every other name stands a little further in, so neighbours do not run into each other when the view is pulled back.
                labels.Add(Label(element.Name, UpgradeTree.OnChart(element.Name, 0f, element.Index % 2 == 0 ? -2.2f : -3.5f) * StepPixels, element.Colour));
                Vector2 mixFoot = UpgradeTree.MixFoot(element.Name, element.MixWith) * StepPixels;
                Color shared = Color.Lerp(element.Colour, Elements.Colour(element.MixWith), 0.5f);
                var knot = Picture("Mix " + element.MixName, emblemSprite, chart, mixFoot, StepPixels * 0.5f);
                knot.color = new Color(shared.r, shared.g, shared.b, 0.75f);
                upright.Add(knot.rectTransform);
                feet.Add(knot.rectTransform);
                mixLabels.Add(Label(element.MixName, UpgradeTree.OnChart(Elements.MixBearing(element.Name, element.MixWith), UpgradeTree.MixDistance, 0f, -0.75f) * StepPixels, new Color(shared.r, shared.g, shared.b, 0.8f)));
            }
            Sprite crown = null;
            iconByName.TryGetValue(UpgradeIcons.Crown, out crown);
            foreach (var mark in marks)
            {
                int kind = (int)mark.Upgrade.Kind;
                mark.Frame = Picture(mark.Upgrade.Name, kind < frames.Length ? frames[kind] : null, chart, mark.Place, mark.Size);
                upright.Add(mark.Frame.rectTransform);
                iconByName.TryGetValue(mark.Upgrade.Icon, out var picture);
                mark.Icon = Picture("Picture", picture, mark.Frame.transform, Vector2.zero, mark.Size * (mark.Upgrade.Kind == UpgradeKind.Spell ? 0.5f : 0.56f));
                // What a god leaves wears a crown over its frame.
                if (mark.Upgrade.Signature && mark.Upgrade.Icon != UpgradeIcons.Crown)
                    Picture("Crown", crown, mark.Frame.transform, new Vector2(0f, mark.Size * 0.62f), mark.Size * 0.42f).color = new Color(1f, 0.85f, 0.45f, 1f);
            }
            // The ring round the spell that is worn; it is moved to whichever that is.
            var badge = Picture("Worn", frames.Length > 0 ? frames[frames.Length - 1] : null, chart, Vector2.zero, StepPixels);
            badge.color = new Color(1f, 0.85f, 0.45f, 0.95f);
            wornBadge = badge.rectTransform;
            wornBadge.gameObject.SetActive(false);
            if (cursor != null) cursor.SetAsLastSibling();
        }

        private Image Picture(string name, Sprite sprite, Transform parent, Vector2 at, float size)
        {
            var made = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)made.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(size, size);
            var image = made.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            if (sprite == null) image.enabled = name != "Picture" && name != "Crown";
            return image;
        }

        private Image Line(Vector2 from, Vector2 to, float thickness)
        {
            var image = Picture("Line", null, chart, from, thickness);
            image.enabled = true;
            var rect = image.rectTransform;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2((to - from).magnitude, thickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
            return image;
        }

        private Text Label(string text, Vector2 at, Color colour)
        {
            var made = new GameObject("Label " + text, typeof(RectTransform));
            var rect = (RectTransform)made.transform;
            rect.SetParent(chart, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(600f, 80f);
            var label = made.AddComponent<Text>();
            label.font = title != null ? title.font : null;
            // Set large and shrunk, so the letters stay sharp when the view is pulled back and they are made larger to be read.
            label.fontSize = 60;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.text = text;
            label.color = colour;
            upright.Add(rect);
            return label;
        }

        /// <summary>Opens the chart on the tree of the element the player holds. The world stops until it is closed.</summary>
        public void Open()
        {
            if (IsOpen || player == null || upgrades == null || panel == null) return;
            // A chart drawn before the playtest was switched on has no slots for what is not built yet: draw it again.
            if (built && slotsDrawn != PlaytestMode.On) Rebuild();
            if (!built) Build();
            ShowTree(upgrades.ElementName);
            IsOpen = true;
            IsWide = false;
            releasePlayer = false;
            openedOnFrame = Time.frameCount;
            stickHeld = false;
            panel.SetActive(true);
            player.Suspended = true;
            Time.timeScale = 0f;
            Show();
            zoom = CloseZoom();
            labelZoom = -1f;
            Place(1f);
        }

        /// <summary>
        /// The playtest's free chart: turns the chart to another element's tree and lets the choice move on it and its
        /// two shared branches, whatever is held. Outside the playtest only the held element's tree is walked.
        /// </summary>
        public bool View(string element)
        {
            if (!IsOpen || !PlaytestMode.On || Elements.ByName(element) == null) return false;
            ShowTree(element);
            Show();
            return true;
        }

        /// <summary>The next tree round the ring (by 1) or the one before (by -1), in the playtest.</summary>
        public bool ViewNext(int by)
        {
            if (!IsOpen || !PlaytestMode.On) return false;
            var at = Elements.ByName(viewed);
            int index = at != null ? at.Index : 0;
            return View(Elements.All[((index + by) % Elements.Count + Elements.Count) % Elements.Count].Name);
        }

        /// <summary>Opens the details view of the chosen mark, in the playtest. True when it opened.</summary>
        public bool OpenDetails()
        {
            var chosen = Chosen;
            if (!IsOpen || !PlaytestMode.On || !MarkDetails.HasDetails(chosen)) return false;
            var page = GuidePage.Ensure();
            if (page == null) return false;
            DetailsOpened++;
            page.OpenOne(MarkDetails.For(chosen, PlaytestTexts.Get()));
            return true;
        }

        /// <summary>Opens the guide to effects over the chart, in the playtest. True when it opened.</summary>
        public bool OpenGuide()
        {
            if (!IsOpen || !PlaytestMode.On) return false;
            var page = GuidePage.Ensure();
            if (page == null) return false;
            GuideOpened++;
            page.OpenGuide();
            return true;
        }

        private void ShowTree(string tree)
        {
            string held = tree;
            viewed = tree;
            bearing = Elements.Bearing(held);
            treeMiddle = new Vector2(0f, (UpgradeTree.EmblemDistance + 5.2f) * StepPixels);
            // The choice moves among the marks of the held tree and of the two branches it shares, as they lie once the tree stands upright.
            string was = heldShown == held && Chosen != null ? Chosen.Id : null;
            shown.Clear();
            places.Clear();
            int root = 0;
            for (int i = 0; i < marks.Count; i++)
            {
                var upgrade = marks[i].Upgrade;
                if (!upgrade.BelongsTo(held)) continue;
                if (NotBuiltYet.IsSlot(upgrade) && !PlaytestMode.On) continue;
                if (upgrade.Id == was || (was == null && upgrade.Kind == UpgradeKind.Root && !upgrade.IsMix)) root = shown.Count;
                shown.Add(i);
                places.Add(UpgradeTree.Upright(marks[i].Place, bearing));
            }
            Selected = Mathf.Clamp(root, 0, Mathf.Max(0, shown.Count - 1));
            heldShown = held;
            if (chart != null)
            {
                chart.localRotation = Quaternion.Euler(0f, 0f, bearing);
                var level = Quaternion.Euler(0f, 0f, -bearing);
                for (int i = 0; i < upright.Count; i++) upright[i].localRotation = level;
            }
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

        /// <summary>
        /// Learns the chosen upgrade if it can be reached and the embers are there; on a spell already learned, wears
        /// it instead. Returns true when something was learned.
        /// </summary>
        public bool LearnSelected()
        {
            var chosen = Chosen;
            if (!IsOpen || chosen == null) return false;
            bool learned = upgrades.TryBuy(chosen);
            if (!learned && chosen.IsSpell && upgrades.Has(chosen.Id)) upgrades.Wear(chosen.Id);
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

        /// <summary>Puts the choice on the named upgrade, if it is among those the choice can reach. Returns true when it is.</summary>
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

        /// <summary>Pulls the view back to show the whole ring, or brings it close again.</summary>
        public void ToggleWide()
        {
            if (!IsOpen) return;
            IsWide = !IsWide;
            if (words != null) words.SetActive(!IsWide);
            Show();
        }

        private Vector2 ScreenSize() => panel != null ? ((RectTransform)panel.transform).rect.size : new Vector2(1920f, 1080f);

        // Close, a step is its full size, or less on a screen too short to hold a tree; pulled back, the whole ring fits.
        private float CloseZoom() => Mathf.Clamp(ScreenSize().y / 1080f, 0.5f, 1f);

        private float WideZoom()
        {
            Vector2 screen = ScreenSize();
            float reach = (UpgradeTree.EmblemDistance + UpgradeTree.TreeHeight + 0.8f) * StepPixels;
            return Mathf.Min(screen.x - 60f, screen.y - 110f) * 0.5f / reach;
        }

        private Vector2 ViewTarget(float atZoom)
        {
            if (IsWide) return WideCentre;
            Vector2 chosen = Selected < places.Count ? places[Selected] : treeMiddle;
            // A narrow screen has no room to keep the tree clear of the words; a wide one has room to spare.
            Vector2 centre = new Vector2(ViewCentre.x * Mathf.Clamp01(ScreenSize().x / 1920f), ViewCentre.y);
            return centre - Vector2.Lerp(treeMiddle, chosen, FollowChoice) * atZoom;
        }

        // Moves the view a share of the way to where it should be; 1 puts it there at once.
        private void Place(float share)
        {
            if (chart == null) return;
            float wanted = IsWide ? WideZoom() : CloseZoom();
            zoom = Mathf.Lerp(zoom, wanted, share);
            chart.localScale = new Vector3(zoom, zoom, 1f);
            chart.anchoredPosition = Vector2.Lerp(chart.anchoredPosition, ViewTarget(zoom), share);
            if (Mathf.Abs(zoom - labelZoom) > 0.0005f)
            {
                // Names keep their size on the screen however far back the view is.
                labelZoom = zoom;
                float scale = Mathf.Lerp(LabelScale * 0.8f, LabelScale, Mathf.InverseLerp(0.3f, 0.8f, zoom)) / zoom;
                for (int i = 0; i < labels.Count; i++) labels[i].rectTransform.localScale = new Vector3(scale, scale, 1f);
                // The names of the shared branches are for the close view; from far off there is no room for them.
                float near = Mathf.InverseLerp(0.45f, 0.75f, zoom);
                for (int i = 0; i < mixLabels.Count; i++)
                {
                    mixLabels[i].rectTransform.localScale = new Vector3(scale, scale, 1f);
                    var colour = mixLabels[i].color;
                    colour.a = 0.8f * near;
                    mixLabels[i].color = colour;
                }
                // The feet of the trees are drawn larger from far off, so the ring can be read.
                float swell = Mathf.Clamp(0.5f / zoom, 1f, 2.4f);
                for (int i = 0; i < feet.Count; i++) feet[i].localScale = new Vector3(swell, swell, 1f);
            }
            // The sky drifts a little against the chart, so the chart is felt to move over it.
            if (sky != null) sky.anchoredPosition = chart.anchoredPosition * 0.04f;
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
            Place(1f - Mathf.Exp(-ViewSpeed * step));
            // The details or the guide lie over the chart and have the buttons while open, and on the frame they close.
            if (GuidePage.BusyThisFrame)
            {
                stickHeld = true;
                return;
            }
            if (cursor != null)
            {
                float beat = 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 5f);
                cursor.localScale = new Vector3(beat, beat, 1f);
            }
            // The press that opened it belongs to the rest fire, not to the first mark on the chart.
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;

            if (hintVersion != ControlDevices.Version) ShowHint();
            Vector2 stick = moveAction.ReadValue<Vector2>();
            bool pushed = stick.sqrMagnitude > 0.25f;
            if (pushed && !stickHeld)
            {
                untilRepeat = RepeatSeconds * 1.7f;
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
            else if (heavyAction != null && heavyAction.WasPressedThisFrame()) ToggleWide();
            else if (PlaytestMode.On)
            {
                if (spellAction != null && spellAction.WasPressedThisFrame()) OpenDetails();
                else if (flaskAction != null && flaskAction.WasPressedThisFrame()) OpenGuide();
                else if (!IsWide && lightAction != null && lightAction.WasPressedThisFrame()) ViewNext(1);
                else if (!IsWide && guardAction != null && guardAction.WasPressedThisFrame()) ViewNext(-1);
            }
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            if (hint == null) return;
            string move = ControlDevices.Current == ControlDevice.Keys ? ControlNames.Of("Move") : "Stick or d-pad";
            if (PlaytestMode.On)
            {
                // Two lines in the playtest: the chart's own buttons, then the playtest's.
                hint.text = IsWide
                    ? ControlNames.Expand("{HeavyAttack}: back to the tree     {Flask}: guide to effects     {Dodge}: leave")
                    : move + ControlNames.Expand(": move     {Interact}: learn, or wear a spell     {HeavyAttack}: the whole chart     {Dodge}: leave\n"
                        + "{Spell}: details     {Guard} / {LightAttack}: tree before / next     {Flask}: guide to effects");
                return;
            }
            hint.text = IsWide
                ? ControlNames.Expand("{HeavyAttack}: back to your tree     {Dodge}: leave")
                : move + ControlNames.Expand(": move along the tree     {Interact}: learn, or wear a spell     {HeavyAttack}: the whole chart     {Dodge}: leave");
        }

        private static string KindWords(Upgrade upgrade)
        {
            if (upgrade.Signature) return "A god's gift";
            if (upgrade.IsMix) return Elements.MixName(upgrade.Element, upgrade.Element2) + (upgrade.IsSpell ? " spell" : " mix");
            switch (upgrade.Kind)
            {
                case UpgradeKind.Root: return "Root";
                case UpgradeKind.Minor: return "Minor";
                case UpgradeKind.Major: return "Major";
                case UpgradeKind.Spell: return "Spell";
                case UpgradeKind.Keystone: return "Keystone";
                default: return "";
            }
        }

        private void Show()
        {
            string held = viewed ?? upgrades.ElementName;
            string holding = upgrades.ElementName;
            if (title != null)
                title.text = PlaytestMode.On
                    ? (held == holding ? held : $"{held}      (you hold {holding})") + $"      Shards  {upgrades.Points}      Playtest: everything open"
                    : $"{held}      Shards  {upgrades.Points}";
            ShowHint();
            for (int i = 0; i < emblems.Count; i++)
            {
                var element = Elements.All[i];
                bool ours = element.Name == held;
                var colour = element.Colour;
                colour.a = ours ? 1f : 0.7f;
                emblems[i].color = colour;
                emblemIcons[i].color = ours ? Color.white : new Color(1f, 1f, 1f, 0.75f);
            }
            Upgrade worn = upgrades.Worn;
            bool wornShown = false;
            foreach (var mark in marks)
            {
                var upgrade = mark.Upgrade;
                bool ours = upgrade.BelongsTo(held);
                bool open = ours && upgrades.Open(upgrade);
                bool owned = ours && upgrades.Owns(upgrade.Id);
                bool reachable = open && !upgrade.Signature && UpgradeTree.Reachable(upgrade, RunState.Unlocked);
                Color frame, picture;
                if (NotBuiltYet.IsSlot(upgrade))
                {
                    frame = NotBuiltColour;
                    if (!ours) frame.a *= IsWide ? WideTrees : OtherTrees;
                    picture = frame;
                }
                else if (!ours)
                {
                    // Another element's tree: there to be seen, faint, in its own colour.
                    frame = Color.Lerp(OutOfReach, mark.Colour, 0.55f);
                    // Pulled back, every tree is there to be looked at; close, the others stand back.
                    frame.a = IsWide ? WideTrees : OtherTrees;
                    picture = frame;
                }
                else if (owned)
                {
                    frame = mark.Colour;
                    picture = Color.white;
                }
                else
                {
                    // In the playtest a god's gift can be bought: it is drawn nearer the colour of what is ready to learn.
                    frame = upgrade.Signature ? (PlaytestMode.On ? Color.Lerp(GodsGift, Affordable, 0.5f) : GodsGift)
                        : !open ? Sealed : !reachable ? OutOfReach : upgrades.Points >= upgrade.Cost ? Affordable : TooDear;
                    picture = frame;
                }
                mark.Frame.color = frame;
                mark.Icon.color = picture;
                for (int i = 0; i < mark.LineCount; i++)
                {
                    // A line is lit once what stands at its far end is learned; it shows the way open when its near end is.
                    var line = !ours ? frame : owned ? mark.Colour : reachable ? TooDear : !open ? Sealed : OutOfReach;
                    line.a = !ours ? (IsWide ? WideTrees : OtherTrees) * 0.7f : owned ? 0.95f : 0.6f;
                    lines[mark.FirstLine + i].color = line;
                }
                if (worn != null && upgrade == worn && wornBadge != null)
                {
                    wornShown = true;
                    wornBadge.anchoredPosition = mark.Place;
                    wornBadge.sizeDelta = new Vector2(mark.Size + 20f, mark.Size + 20f);
                }
            }
            if (wornBadge != null && wornBadge.gameObject.activeSelf != wornShown) wornBadge.gameObject.SetActive(wornShown);

            var chosen = Chosen;
            if (chosen == null) return;
            var chosenMark = marks[shown[Selected]];
            if (cursor != null)
            {
                cursor.anchoredPosition = chosenMark.Place;
                cursor.localRotation = Quaternion.Euler(0f, 0f, -bearing);
                cursor.sizeDelta = new Vector2(chosenMark.Size + 26f, chosenMark.Size + 26f);
            }
            if (chosenName != null) chosenName.text = chosen.Name;
            if (chosenState != null)
            {
                bool owned = upgrades.Owns(chosen.Id);
                string kind = KindWords(chosen);
                string cost = chosen.Signature ? "" : chosen.Cost == 1 ? "     1 Shard" : $"     {chosen.Cost} Shards";
                string state;
                if (NotBuiltYet.IsSlot(chosen))
                {
                    kind = NotBuiltYet.Label;
                    cost = "";
                    state = "A place kept for what is coming.";
                }
                else if (chosen.Signature && PlaytestMode.On) state = owned ? "Learned (a playtest unlock)." : "Playtest unlock: learn it here. " + NotBuiltYet.GiftSource(chosen.Element);
                else if (chosen.Signature) state = owned ? "Left to you by a god." : "Only a god's fall gives this.";
                else if (owned && chosen.IsSpell) state = worn == chosen ? "Worn. Hold the spell button to cast it." : "Learned. Press learn to wear it.";
                else if (owned && PlaytestMode.On && !upgrades.Has(chosen.Id)) state = "Learned. In force while you hold " + (chosen.IsMix ? chosen.Element + " or " + chosen.Element2 : chosen.Element) + (string.IsNullOrEmpty(chosen.Weapon) ? "." : ", with the " + chosen.Weapon + ".");
                else if (owned) state = "Learned.";
                else if (!upgrades.Open(chosen)) state = $"Sealed. Hold or consume {(chosen.Element == held ? chosen.Element2 : chosen.Element)} in this run to open it.";
                else if (!UpgradeTree.Reachable(chosen, RunState.Unlocked)) state = "Grows from " + Parents(chosen) + ".";
                else state = upgrades.Points >= chosen.Cost ? "Can be learned." : $"You carry {upgrades.Points}.";
                if (PlaytestMode.On && MarkDetails.HasDetails(chosen)) state += "\n" + ControlNames.Expand("{Spell}: details");
                chosenState.text = kind + cost + "\n" + state;
            }
            if (description != null)
            {
                string needs = string.IsNullOrEmpty(chosen.Weapon) ? "" : $"\nNeeds the {chosen.Weapon}.";
                description.text = chosen.Description + needs;
            }
        }

        private static string Parents(Upgrade upgrade)
        {
            string names = "";
            foreach (string id in upgrade.Requires)
            {
                var parent = Upgrade.ById(id);
                if (parent == null) continue;
                names += names.Length == 0 ? parent.Name : " or " + parent.Name;
            }
            return names;
        }
    }
}
