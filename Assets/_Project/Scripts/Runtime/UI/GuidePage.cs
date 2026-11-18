using System.Collections.Generic;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The playtest's reading page, over everything else on the screen: the guide to effects (a list of entries on the
    /// left, the chosen one's words on the right), a mark's details, what is new, and the title shown on arriving at
    /// the Proving Ground. The player reads at their own pace: words too long for the page are split into pages, the
    /// use button (or a click) turns the page, the dodge button (or Esc, or Start) goes back. The stick, the d-pad, the
    /// keys, the mouse and its wheel choose an entry. The world stays stopped while it is open. It is made by code the
    /// first time it is wanted, under the HUD of whatever scene the player is in, so no scene had to be built again.
    /// </summary>
    public class GuidePage : MonoBehaviour
    {
        public static GuidePage Instance { get; private set; }
        /// <summary>True while the page is open.</summary>
        public static bool Showing => Instance != null && Instance.IsOpen;
        /// <summary>True while the page is open or was closed this frame: the menus under it leave their buttons alone then.</summary>
        public static bool BusyThisFrame => Instance != null && (Instance.IsOpen || Instance.closedOnFrame == Time.frameCount);

        public bool IsOpen { get; private set; }
        /// <summary>True while it shows the guide's list; false for one text alone.</summary>
        public bool IsList { get; private set; }
        public bool IsTitle { get; private set; }
        public int Selected { get; private set; }
        public int Page { get; private set; }
        public int PageCount => pages.Count;
        public string ShownTitle => title != null ? title.text : "";
        public string ShownWords => body != null ? body.text : "";
        public int EntryCount => entries.Count;
        public int Opened { get; private set; }

        public float RepeatSeconds = 0.2f;
        public Color Plain = new Color(0.95f, 0.93f, 0.9f, 1f);
        public Color Gold = new Color(1f, 0.8f, 0.45f, 1f);
        public Color Faint = new Color(1f, 1f, 1f, 0.6f);

        private const int VisibleRows = 16;
        private const float RowHeight = 46f;
        private const float ListTop = -122f;
        private const float BodyTop = -122f;
        private const float BodyHeight = 690f;

        private PlayerController player;
        private InputAction moveAction, interactAction, dodgeAction, pauseAction, alwaysBack;
        private Text title, banner, body, pageMark, hint;
        private RectTransform bodyRect, cursor, listRoot;
        private readonly List<Text> rows = new List<Text>();
        // The guide's list as shown: group headings (not chosen) and entries, by their place in entries.
        private readonly List<int> shownEntry = new List<int>();
        private readonly List<string> shownLabel = new List<string>();
        private readonly List<PlaytestText> entries = new List<PlaytestText>();
        private readonly List<string> pages = new List<string>();
        private PlaytestText single;
        private int openedOnFrame = -1, closedOnFrame = -1;
        private float savedScale = 1f;
        private bool savedSuspended;
        private int heldDirection;
        private float untilRepeat;
        private int hintVersion = -1;
        private int firstRow;
        private float bodyHeight = BodyHeight;

        // Domain reload is off: the page of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>The page for this scene, made under the HUD the first time it is asked for. Null when the scene has no HUD.</summary>
        public static GuidePage Ensure()
        {
            if (Instance != null) return Instance;
            Transform hud = PauseMenu.Instance != null ? PauseMenu.Instance.transform : UpgradeMenu.Instance != null ? UpgradeMenu.Instance.transform : null;
            if (hud == null)
            {
                var found = FindAnyObjectByType<PlayerHud>();
                hud = found != null ? found.transform : null;
            }
            var player = FindAnyObjectByType<PlayerController>();
            if (hud == null || player == null) return null;
            var made = new GameObject("Guide Page", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            var page = made.AddComponent<GuidePage>();
            page.Build(hud, player);
            Instance = page;
            return page;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
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
            alwaysBack = new InputAction("Guide Back", InputActionType.Button);
            foreach (string path in ControlBindings.Reserved) alwaysBack.AddBinding(path);
            alwaysBack.Enable();

            // The HUD's own font and inked frame, so the page is one of the game's menus.
            var anyText = hud.GetComponentInChildren<Text>(true);
            Font font = anyText != null ? anyText.font : null;
            var framed = hud.Find("Pause Menu/Panel");
            Sprite frame = framed != null && framed.GetComponent<Image>() != null ? framed.GetComponent<Image>().sprite : null;
            var brushed = hud.Find("Pause Menu/Panel/List/Cursor");
            Sprite brush = brushed != null && brushed.GetComponent<Image>() != null ? brushed.GetComponent<Image>().sprite : null;

            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.008f, 0.03f, 0.93f);
            dim.raycastTarget = false;

            // Solid ink under the frame: nothing of the chart or the world shows through the words.
            var backing = Rect("Backing", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1612f, 892f));
            var backingImage = backing.gameObject.AddComponent<Image>();
            backingImage.color = new Color(0.02f, 0.018f, 0.05f, 1f);
            backingImage.raycastTarget = false;
            var panel = Rect("Panel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1640f, 920f));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = frame;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = frame != null ? Color.white : new Color(0.03f, 0.025f, 0.06f, 1f);
            panelImage.raycastTarget = false;
            var edge = Rect("Edge", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -86f), new Vector2(1560f, 2f));
            var edgeImage = edge.gameObject.AddComponent<Image>();
            edgeImage.color = new Color(0.8f, 0.72f, 0.55f, 0.7f);
            edgeImage.raycastTarget = false;

            title = Label("Title", panel, font, 40, TextAnchor.UpperLeft, Gold, new Vector2(44f, -26f), new Vector2(1500f, 50f));
            banner = Label("Banner", panel, font, 92, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.86f, 1f), Vector2.zero, new Vector2(1560f, 100f));
            banner.rectTransform.anchorMin = banner.rectTransform.anchorMax = banner.rectTransform.pivot = new Vector2(0.5f, 1f);
            banner.rectTransform.anchoredPosition = new Vector2(0f, -4f);
            banner.text = "Project Clover";
            banner.gameObject.SetActive(false);

            listRoot = Rect("List", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(560f, 800f));
            cursor = Rect("Cursor", listRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, ListTop), new Vector2(540f, 44f));
            var cursorImage = cursor.gameObject.AddComponent<Image>();
            cursorImage.sprite = brush;
            cursorImage.type = Image.Type.Sliced;
            cursorImage.color = new Color(1f, 0.62f, 0.2f, 0.3f);
            cursorImage.raycastTarget = false;
            for (int i = 0; i < VisibleRows; i++)
                rows.Add(Label("Row " + (i + 1), listRoot, font, 28, TextAnchor.MiddleLeft, Plain, new Vector2(44f, ListTop - i * RowHeight), new Vector2(520f, RowHeight - 4f)));
            var rule = Rect("Rule", listRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(586f, -112f), new Vector2(2f, 720f));
            var ruleImage = rule.gameObject.AddComponent<Image>();
            ruleImage.color = new Color(0.8f, 0.72f, 0.55f, 0.35f);
            ruleImage.raycastTarget = false;

            body = Label("Body", panel, font, 29, TextAnchor.UpperLeft, Plain, new Vector2(620f, BodyTop), new Vector2(970f, BodyHeight));
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            body.lineSpacing = 1.12f;
            body.supportRichText = true;
            bodyRect = body.rectTransform;
            pageMark = Label("Page", panel, font, 24, TextAnchor.LowerRight, Faint, new Vector2(1060f, -834f), new Vector2(530f, 30f));
            hint = Label("Hint", panel, font, 22, TextAnchor.LowerLeft, Faint, new Vector2(48f, -876f), new Vector2(1500f, 30f));

            gameObject.SetActive(false);
            transform.SetAsLastSibling();
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 at, Vector2 size)
        {
            var made = new GameObject(name, typeof(RectTransform));
            made.transform.SetParent(parent, false);
            var rect = (RectTransform)made.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = at;
            rect.sizeDelta = size;
            return rect;
        }

        private static Text Label(string name, Transform parent, Font font, int size, TextAnchor anchor, Color colour, Vector2 at, Vector2 box)
        {
            var rect = Rect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), at, box);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            // Sizes are given as for a narrow face, as the HUD's builder gives them.
            label.fontSize = Mathf.RoundToInt(size * 0.86f);
            label.alignment = anchor;
            label.color = colour;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        /// <summary>Opens the guide to effects on its first entry, or on the entry with the given id.</summary>
        public void OpenGuide(string startAt = null)
        {
            var texts = PlaytestTexts.Get();
            entries.Clear();
            shownEntry.Clear();
            shownLabel.Clear();
            string group = null;
            if (texts != null)
                foreach (var entry in texts.Effects)
                {
                    if (entry == null) continue;
                    if (entry.Group != group)
                    {
                        group = entry.Group;
                        shownEntry.Add(-1);
                        shownLabel.Add(string.IsNullOrEmpty(group) ? "" : group.ToUpperInvariant());
                    }
                    shownEntry.Add(entries.Count);
                    shownLabel.Add(entry.Title);
                    entries.Add(entry);
                }
            if (entries.Count == 0)
            {
                OpenOne(new PlaytestText { Title = "Guide to effects", Body = "The guide's words have not been built. In the editor: Clover > Build > Playtest Texts." });
                return;
            }
            IsList = true;
            IsTitle = false;
            single = null;
            Selected = 0;
            if (!string.IsNullOrEmpty(startAt))
                for (int i = 0; i < entries.Count; i++)
                    if (entries[i].Id == startAt) Selected = i;
            firstRow = 0;
            Begin();
            title.text = "Guide to effects";
            ShowEntry();
        }

        /// <summary>Opens one text alone: a mark's details, or what is new.</summary>
        public void OpenOne(PlaytestText entry)
        {
            if (entry == null) return;
            IsList = false;
            IsTitle = false;
            single = entry;
            Begin();
            title.text = entry.Title;
            Paginate(entry.Body);
            ShowPage(0);
        }

        /// <summary>The title as the Proving Ground opens: the game's name, the playtest's version and what is new.</summary>
        public void OpenTitle()
        {
            var texts = PlaytestTexts.Get();
            string version = texts != null && !string.IsNullOrEmpty(texts.Version) ? texts.Version : "Combat Playtest";
            OpenOne(new PlaytestText { Id = "title", Title = version + "  ·  What's new", Body = texts != null ? texts.WhatsNew : "" });
            IsTitle = true;
            // The name large across the top, the version under the rule, and what is new below it.
            banner.gameObject.SetActive(true);
            title.alignment = TextAnchor.UpperCenter;
            title.rectTransform.anchoredPosition = new Vector2(44f, -104f);
            bodyRect.anchoredPosition = new Vector2(160f, -176f);
            bodyRect.sizeDelta = new Vector2(1320f, 620f);
            bodyHeight = 620f;
            Paginate(texts != null ? texts.WhatsNew : "");
            ShowPage(0);
            ShowHint();
        }

        /// <summary>What is new in this version of the playtest.</summary>
        public void OpenWhatsNew()
        {
            var texts = PlaytestTexts.Get();
            string version = texts != null && !string.IsNullOrEmpty(texts.Version) ? texts.Version : "Combat Playtest";
            OpenOne(new PlaytestText { Id = "whats_new", Title = version + "      What's new", Body = texts != null ? texts.WhatsNew : "Not built." });
        }

        private void Begin()
        {
            Opened++;
            if (!IsOpen)
            {
                savedScale = Time.timeScale;
                savedSuspended = player != null && player.Suspended;
            }
            IsOpen = true;
            openedOnFrame = Time.frameCount;
            heldDirection = 0;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            banner.gameObject.SetActive(false);
            listRoot.gameObject.SetActive(IsList);
            title.alignment = TextAnchor.UpperLeft;
            title.rectTransform.anchoredPosition = new Vector2(44f, -26f);
            bodyRect.anchoredPosition = new Vector2(IsList ? 620f : 60f, BodyTop);
            bodyRect.sizeDelta = new Vector2(IsList ? 970f : 1520f, BodyHeight);
            bodyHeight = BodyHeight;
            if (player != null) player.Suspended = true;
            Time.timeScale = 0f;
            hintVersion = -1;
            ShowHint();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            IsTitle = false;
            closedOnFrame = Time.frameCount;
            gameObject.SetActive(false);
            // Back to what was under it: a menu keeps the world stopped and the player held; the open world gets both back.
            Time.timeScale = savedScale;
            if (player != null) player.Suspended = savedSuspended;
        }

        /// <summary>The use button: the next page, or on the last page of a text alone, back.</summary>
        public void Next()
        {
            if (!IsOpen) return;
            if (Page + 1 < pages.Count) ShowPage(Page + 1);
            else if (!IsList) Close();
            else ShowPage(0);
        }

        /// <summary>Moves the choice in the guide's list.</summary>
        public void Move(int by)
        {
            if (!IsOpen || !IsList || entries.Count == 0) return;
            Selected = ((Selected + by) % entries.Count + entries.Count) % entries.Count;
            ShowEntry();
        }

        private void ShowEntry()
        {
            var entry = entries[Selected];
            Paginate("<color=#E8C77A>" + entry.Title + "</color>\n\n" + entry.Body);
            ShowPage(0);
            // The list scrolls to keep the chosen entry (and its group's heading above it) in view.
            int shownAt = shownEntry.IndexOf(Selected);
            if (shownAt - 1 < firstRow) firstRow = Mathf.Max(0, shownAt - 1);
            if (shownAt >= firstRow + VisibleRows) firstRow = shownAt - VisibleRows + 1;
            for (int i = 0; i < rows.Count; i++)
            {
                int index = firstRow + i;
                bool used = index < shownLabel.Count;
                rows[i].enabled = used;
                if (!used) continue;
                bool heading = shownEntry[index] < 0;
                rows[i].text = heading ? shownLabel[index] : "   " + shownLabel[index];
                rows[i].color = heading ? Gold : Plain;
                rows[i].fontSize = heading ? 20 : 24;
            }
            cursor.anchoredPosition = new Vector2(28f, ListTop - (shownAt - firstRow) * RowHeight);
        }

        // The words split at paragraphs into pages that fit the box; a paragraph is never cut.
        private void Paginate(string words)
        {
            pages.Clear();
            if (string.IsNullOrEmpty(words))
            {
                pages.Add("");
                return;
            }
            // Paragraphs, and inside a list each of its lines, are the pieces a page may end after.
            var parts = new List<string>();
            var joins = new List<string>();
            foreach (string paragraph in words.Replace("\r", "").Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var lines = paragraph.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    parts.Add(lines[i]);
                    joins.Add(i == 0 ? "\n\n" : "\n");
                }
            }
            var settings = body.GetGenerationSettings(new Vector2(bodyRect.sizeDelta.x, 0f));
            var generator = body.cachedTextGeneratorForLayout;
            float scale = body.pixelsPerUnit > 0f ? body.pixelsPerUnit : 1f;
            string page = "";
            for (int index = 0; index < parts.Count; index++)
            {
                string part = parts[index];
                string next = page.Length == 0 ? part : page + joins[index] + part;
                float height = generator.GetPreferredHeight(next, settings) / scale;
                if (height > bodyHeight - 10f && page.Length > 0)
                {
                    pages.Add(page);
                    page = part;
                }
                else page = next;
            }
            pages.Add(page);
        }

        private void ShowPage(int index)
        {
            Page = Mathf.Clamp(index, 0, pages.Count - 1);
            body.text = pages[Page];
            string turn = ControlNames.Of("Interact");
            if (pages.Count > 1) pageMark.text = $"Page {Page + 1} of {pages.Count}     {turn}: {(Page + 1 < pages.Count ? "next page" : IsList ? "first page" : "close")}";
            else pageMark.text = IsList ? "" : $"{turn}: close";
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            if (hint == null) return;
            var style = ControlDevices.Current;
            string choose = style == ControlDevice.Keys ? "W S, the wheel or the mouse" : "Stick or d-pad";
            string interact = ControlNames.Of("Interact"), back = ControlNames.Of("Dodge");
            if (IsTitle) hint.text = $"{interact} or a click: {(pages.Count > 1 ? "read on" : "begin")}     {back}: begin";
            else if (IsList) hint.text = $"{choose}: choose     {interact} or a click: next page     {back}: back";
            else hint.text = $"{interact} or a click: {(pages.Count > 1 ? "next page" : "close")}     {back}: back";
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            if (hintVersion != ControlDevices.Version) ShowHint();
            bool back = dodgeAction.WasPressedThisFrame() || (pauseAction != null && pauseAction.WasPressedThisFrame()) || (alwaysBack != null && alwaysBack.WasPressedThisFrame());
            if (back)
            {
                Close();
                return;
            }
            if (interactAction.WasPressedThisFrame())
            {
                Next();
                return;
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (IsList && wheel != 0f) Move(wheel > 0f ? -1 : 1);
                if (mouse.leftButton.wasPressedThisFrame && !ClickRow(mouse.position.ReadValue()))
                {
                    Next();
                    return;
                }
            }
            if (!IsList) return;
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

        // A click on an entry in the list chooses it. True when it landed on one.
        private bool ClickRow(Vector2 at)
        {
            if (!IsList) return false;
            for (int i = 0; i < rows.Count; i++)
            {
                if (!rows[i].enabled || !RectTransformUtility.RectangleContainsScreenPoint(rows[i].rectTransform, at, null)) continue;
                int index = firstRow + i;
                if (index >= shownEntry.Count || shownEntry[index] < 0) return true;
                Selected = shownEntry[index];
                ShowEntry();
                return true;
            }
            return false;
        }
    }
}
