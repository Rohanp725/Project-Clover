using System.Collections.Generic;
using System.Text;
using Clover.Core;
using Clover.Player;
using Clover.Player.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The Hand (STEEL_AND_ELEMENT.md 5.4, IMPLEMENTATION.md 4.1), opened from a rest fire's menu: the four spell slots in
    /// a diamond laid out like the buttons (each named by its effective binding), the surge in the middle, the elements
    /// carried above (picked in the hub; in the playtest at the altars: the Hand never changes them), a suggested Hand
    /// and three saved Hands beside. Choosing a slot lists only what can go there, grouped by element, each with its role
    /// and one plain line; the details button adds the numbers. Every change is worn at once through
    /// <see cref="PlayerBuild.Apply"/>. Read at the player's own pace: the stick, the d-pad, the keys, the mouse and its
    /// wheel choose; use (or a click) acts; the heavy attack (or a right click) keeps the Hand on a saved Hand's row; the
    /// dodge button (or Esc, or Start) backs out. The world stays stopped. Made by code the first time it is wanted,
    /// under the HUD, as <see cref="GuidePage"/> is.
    /// </summary>
    public class HandMenu : MonoBehaviour
    {
        public static HandMenu Instance { get; private set; }
        public static bool Showing => Instance != null && Instance.IsOpen;
        /// <summary>True while open or closed this frame: the rest menu under it leaves its buttons alone then.</summary>
        public static bool BusyThisFrame => Instance != null && (Instance.IsOpen || Instance.closedOnFrame == Time.frameCount);

        /// <summary>Where the cursor rests: the four slots (0 to 3, by button), the surge, the suggested Hand, the three saved Hands.</summary>
        public const int SurgeItem = 4, SuggestedItem = 5, FirstSavedItem = 6, ItemCount = 9;

        public float RepeatSeconds = 0.2f;

        public bool IsOpen { get; private set; }
        /// <summary>True while a slot's (or the surge's) choices are listed.</summary>
        public bool Choosing { get; private set; }
        /// <summary>The item the cursor is on (see <see cref="SurgeItem"/>), and in a list, the choice.</summary>
        public int Item { get; private set; }
        public int Choice { get; private set; }
        public bool Details { get; private set; }
        public IReadOnlyList<HandChoice> Choices => choices;
        // Counts for probes.
        public int Opened { get; private set; }
        public int Placed { get; private set; }
        public int HandsKept { get; private set; }
        public int HandsWorn { get; private set; }

        private struct SlotBox
        {
            public RectTransform Rect;
            public Image Back, Rim;
            public Text Key, Name, Line;
        }

        private PlayerController player;
        private PlayerBuild build;
        private InputAction moveAction, interactAction, dodgeAction, pauseAction, heavyAction, detailsAction, alwaysBack;
        private Text title, carried, infoTitle, infoBody, choiceLine, hint;
        private RectTransform listRoot, listCursor, rowCursor, handCursor;
        private readonly SlotBox[] boxes = new SlotBox[SurgeItem + 1];
        private readonly Text[] actionRows = new Text[ItemCount - SuggestedItem];
        private readonly List<Text> listRows = new List<Text>();
        // What the list shows, row by row: a choice (its index in choices) or a heading (-1, its words in listLabels).
        private readonly List<int> listEntry = new List<int>();
        private readonly List<string> listLabels = new List<string>();
        private readonly List<HandChoice> choices = new List<HandChoice>();
        private readonly StringBuilder words = new StringBuilder(256);
        private int openedOnFrame = -1, closedOnFrame = -1;
        private float savedScale = 1f;
        private bool savedSuspended;
        private int heldDirection;
        private float untilRepeat;
        private int firstRow;
        private int shownVersion = -1, hintVersion = -1;

        // The list begins under the heading on the right ("Choose for West (X)", -120 to -164) and ends above the chosen one's line (-720).
        private const int VisibleRows = 12;
        private const float RowHeight = 44f;
        private const float ListTop = -176f;
        // The frame round the box the cursor is on: the boxes are opaque, so it must stand out past their edges.
        private const float CursorMargin = 7f;
        private static readonly Color CursorColour = new Color(1f, 0.8f, 0.45f, 0.95f);
        private static readonly Color ChosenInk = new Color(0.16f, 0.1f, 0.04f, 1f);
        private static readonly string[] SlotWords = { "West", "North", "East", "South" };
        // Where each slot's box stands in the diamond, as the face buttons do: West, North, East, South, then the surge in the middle.
        private static readonly Vector2[] BoxAt = { new Vector2(40f, -380f), new Vector2(300f, -200f), new Vector2(560f, -380f), new Vector2(300f, -560f), new Vector2(320f, -390f) };
        private static readonly Vector2 BoxSize = new Vector2(250f, 120f), SurgeSize = new Vector2(210f, 100f);

        // Domain reload is off: the menu of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>The Hand for this scene, made under the HUD the first time it is asked for. Null when the scene has no HUD or player.</summary>
        public static HandMenu Ensure()
        {
            if (Instance != null) return Instance;
            var hud = MenuParts.FindHud();
            var player = FindAnyObjectByType<PlayerController>();
            if (hud == null || player == null) return null;
            var made = new GameObject("Hand Menu", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            var menu = made.AddComponent<HandMenu>();
            menu.Build(hud, player);
            Instance = menu;
            return menu;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            // A scene change with the Hand open must not leave the world stopped. What it saved may be the rest menu's
            // stopped state, and the two are torn down in no set order, so this goes straight back to running.
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
            heavyAction = player.FindAction("HeavyAttack");
            detailsAction = player.FindAction("WeaponSkill");
            alwaysBack = new InputAction("Hand Back", InputActionType.Button);
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

            title = MenuParts.Label("Title", panel, font, 40, TextAnchor.UpperLeft, MenuParts.Gold, new Vector2(44f, -26f), new Vector2(700f, 50f));
            title.text = "The Hand";
            carried = MenuParts.Label("Carried", panel, font, 26, TextAnchor.UpperRight, MenuParts.Plain, new Vector2(760f, -34f), new Vector2(840f, 40f));
            carried.supportRichText = true;

            // The diamond, on the left.
            var diamond = MenuParts.Rect("Diamond", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, 0f), new Vector2(850f, 920f));
            rowCursor = (RectTransform)MenuParts.Box("Box Cursor", diamond, Vector2.zero, BoxSize + new Vector2(CursorMargin * 2f, CursorMargin * 2f), CursorColour).transform;
            for (int i = 0; i <= SurgeItem; i++)
            {
                var size = i == SurgeItem ? SurgeSize : BoxSize;
                var rim = MenuParts.Box(i == SurgeItem ? "Surge" : "Slot " + SlotWords[i], diamond, BoxAt[i], size, MenuParts.Faint);
                var back = MenuParts.Box("Back", rim.transform, new Vector2(4f, -4f), size - new Vector2(8f, 8f), MenuParts.Ink);
                boxes[i] = new SlotBox
                {
                    Rect = rim.rectTransform,
                    Rim = rim,
                    Back = back,
                    Key = MenuParts.Label("Key", rim.transform, font, 20, TextAnchor.UpperLeft, MenuParts.Faint, new Vector2(12f, -6f), new Vector2(size.x - 24f, 26f)),
                    Name = MenuParts.Label("Name", rim.transform, font, 27, TextAnchor.MiddleCenter, MenuParts.Plain, new Vector2(8f, -34f), new Vector2(size.x - 16f, 36f)),
                    Line = MenuParts.Label("Line", rim.transform, font, 19, TextAnchor.UpperCenter, MenuParts.Faint, new Vector2(8f, -72f), new Vector2(size.x - 16f, 28f)),
                };
            }
            // The surge sits over the middle of the diamond, in front of the slots' boxes.
            boxes[SurgeItem].Rect.SetAsLastSibling();

            // The right side: what the cursor is on, and below it the Hands; or, while choosing, the list.
            infoTitle = MenuParts.Label("Info Title", panel, font, 32, TextAnchor.UpperLeft, MenuParts.Gold, new Vector2(920f, -120f), new Vector2(680f, 44f));
            infoBody = MenuParts.Label("Info", panel, font, 25, TextAnchor.UpperLeft, MenuParts.Plain, new Vector2(920f, -172f), new Vector2(670f, 330f));
            infoBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            infoBody.verticalOverflow = VerticalWrapMode.Truncate;
            infoBody.supportRichText = true;
            infoBody.lineSpacing = 1.1f;
            // The brush stroke under the chosen Hand's row, as under a chosen row of the pause menu.
            handCursor = (RectTransform)MenuParts.Box("Hand Cursor", panel, new Vector2(922f, -560f), new Vector2(680f, 46f), new Color(1f, 0.62f, 0.2f, 0.3f), brush).transform;
            for (int i = 0; i < actionRows.Length; i++)
                actionRows[i] = MenuParts.Label("Hand Row " + (i + 1), panel, font, 26, TextAnchor.MiddleLeft, MenuParts.Plain, new Vector2(940f, -560f - i * 56f), new Vector2(660f, 50f));

            listRoot = MenuParts.Rect("List", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(900f, 0f), new Vector2(700f, 920f));
            listCursor = (RectTransform)MenuParts.Box("Cursor", listRoot, new Vector2(10f, ListTop), new Vector2(690f, RowHeight - 4f), new Color(1f, 0.62f, 0.2f, 0.3f), brush).transform;
            for (int i = 0; i < VisibleRows; i++)
            {
                var row = MenuParts.Label("Row " + (i + 1), listRoot, font, 25, TextAnchor.MiddleLeft, MenuParts.Plain, new Vector2(24f, ListTop - i * RowHeight), new Vector2(670f, RowHeight - 4f));
                row.supportRichText = true;
                listRows.Add(row);
            }
            choiceLine = MenuParts.Label("Choice Line", panel, font, 23, TextAnchor.UpperLeft, MenuParts.Faint, new Vector2(920f, -720f), new Vector2(670f, 140f));
            choiceLine.horizontalOverflow = HorizontalWrapMode.Wrap;
            choiceLine.verticalOverflow = VerticalWrapMode.Truncate;
            hint = MenuParts.Label("Hint", panel, font, 22, TextAnchor.LowerLeft, MenuParts.Faint, new Vector2(48f, -876f), new Vector2(1560f, 30f));

            gameObject.SetActive(false);
            transform.SetAsLastSibling();
        }

        /// <summary>Opens the Hand (at a rest fire: it is only ever opened from the rest menu). False when there is no build to change.</summary>
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
            Choosing = false;
            Item = 0;
            openedOnFrame = Time.frameCount;
            heldDirection = 0;
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
            Choosing = false;
            closedOnFrame = Time.frameCount;
            gameObject.SetActive(false);
            Time.timeScale = savedScale;
            player.Suspended = savedSuspended;
        }

        /// <summary>The back button: out of a list to the diamond, or out of the Hand.</summary>
        public void Back()
        {
            if (Choosing)
            {
                Choosing = false;
                Redraw();
            }
            else Close();
        }

        /// <summary>Moves the cursor: over the diamond and the Hands, or down the list while choosing.</summary>
        public void Move(int by)
        {
            if (!IsOpen) return;
            if (Choosing)
            {
                if (choices.Count == 0) return;
                Choice = ((Choice + by) % choices.Count + choices.Count) % choices.Count;
            }
            else Item = ((Item + by) % ItemCount + ItemCount) % ItemCount;
            Redraw();
        }

        /// <summary>The use button: opens a slot's list, wears the choice, or wears the suggested or a saved Hand.</summary>
        public void Use()
        {
            if (!IsOpen) return;
            if (Choosing)
            {
                Place(choices.Count > 0 ? choices[Choice].Id : "");
                return;
            }
            var open = build.Open;
            if (Item < SurgeItem && !open.SlotOpen(Item)) return;
            if (Item <= SurgeItem)
            {
                ListFor(Item);
                Choosing = true;
                Redraw();
                return;
            }
            if (Item == SuggestedItem)
            {
                if (build.Apply(build.Suggested(), true)) HandsWorn++;
            }
            else if (build.LoadHand(Item - FirstSavedItem, true)) HandsWorn++;
            Redraw();
        }

        /// <summary>The heavy attack on a saved Hand's row: keeps the Hand worn now there.</summary>
        public void Keep()
        {
            if (!IsOpen || Choosing || Item < FirstSavedItem) return;
            build.SaveHand(Item - FirstSavedItem);
            HandsKept++;
            Redraw();
        }

        /// <summary>Shows or hides the numbers.</summary>
        public void ToggleDetails()
        {
            Details = !Details;
            Redraw();
        }

        // What can go where the cursor is: for a slot, "nothing" first and then every allowed spell; for the surge, each carried element's.
        private void ListFor(int item)
        {
            var loadout = build.Read();
            if (item == SurgeItem) build.Rules.SurgesFor(loadout, RunState.Unlocked, choices);
            else
            {
                build.Rules.ChoicesFor(item, loadout, RunState.Unlocked, choices);
                choices.Insert(0, new HandChoice { Id = "", Name = "Nothing", Element = "", Element2 = "", Line = "Leave the slot empty.", InSlot = -1 });
            }
            string current = item == SurgeItem ? loadout.Surge : loadout.SlotAt(item);
            Choice = 0;
            for (int i = 0; i < choices.Count; i++)
                if (choices[i].Id == current) Choice = i;
            // The list as shown: a heading before each element's group.
            listEntry.Clear();
            listLabels.Clear();
            string group = null;
            for (int i = 0; i < choices.Count; i++)
            {
                string heading = GroupOf(choices[i]);
                if (heading != group)
                {
                    group = heading;
                    if (heading.Length > 0)
                    {
                        listEntry.Add(-1);
                        listLabels.Add(heading);
                    }
                }
                listEntry.Add(i);
                listLabels.Add("");
            }
            firstRow = 0;
        }

        private static string GroupOf(in HandChoice choice)
        {
            if (string.IsNullOrEmpty(choice.Element)) return "";
            return choice.IsDuo ? (choice.Element + " and " + choice.Element2).ToUpperInvariant() : choice.Element.ToUpperInvariant();
        }

        // Wears a choice in the slot (or as the surge) being chosen for, through the build, which drops what the rules forbid.
        private void Place(string id)
        {
            var hand = build.Read().Clone();
            if (Item == SurgeItem) hand.Surge = id;
            else
            {
                // A spell moved here leaves the slot it was in.
                for (int i = 0; i < Loadout.SlotCount; i++)
                    if (i != Item && hand.Slots[i] == id) hand.Slots[i] = "";
                hand.Slots[Item] = id ?? "";
            }
            if (build.Apply(hand, true)) Placed++;
            Choosing = false;
            Redraw();
        }

        private void Redraw()
        {
            shownVersion = build.Version;
            var loadout = build.Read();
            var open = build.Open;
            var device = ControlDevices.Current;

            // The elements carried, each in its colour.
            words.Clear();
            words.Append("Carried: ");
            if (loadout.First == "") words.Append("no element");
            else
            {
                AppendElement(loadout.First);
                if (loadout.AllIn) words.Append(", all in");
                else if (loadout.Second != "")
                {
                    words.Append(" and ");
                    AppendElement(loadout.Second);
                }
            }
            words.Append(PlaytestMode.On ? "   (set at the altars)" : "   (chosen in the hub)");
            carried.text = words.ToString();

            for (int i = 0; i < SurgeItem; i++)
            {
                string id = loadout.SlotAt(i);
                var box = boxes[i];
                box.Key.text = SlotWords[i] + "   " + ControlNames.Of(LayeredButtons.SlotActions[i], device);
                if (!open.SlotOpen(i))
                {
                    // Short enough for the box; the whole rule is said on the right.
                    Dress(box, "Shut", i == 2 ? "Opens at the Champion" : "Opens at the Fire god", MenuParts.Faint);
                    continue;
                }
                string element = ElementOfSpell(id);
                Dress(box, id.Length == 0 ? "Empty" : NameOf(id), id.Length == 0 ? "" : RoleOf(id), element.Length > 0 ? Elements.Colour(element) : MenuParts.Faint);
            }
            string surgeElement = HandRules.ElementOfSurge(loadout.Surge);
            var surgeKit = surgeElement.Length > 0 ? ElementKit.ByName(surgeElement, false) : null;
            // A duo surge is named by its mark and coloured by its first element.
            var duoSurge = Upgrade.ById(loadout.Surge);
            if (!DuoSurge.Is(duoSurge)) duoSurge = null;
            if (duoSurge != null) surgeElement = duoSurge.Element;
            boxes[SurgeItem].Key.text = "Surge   " + ControlNames.Of("Surge", device);
            Dress(boxes[SurgeItem], duoSurge != null ? duoSurge.Name : surgeKit != null ? surgeKit.SurgeName : "None", duoSurge != null ? duoSurge.Element + " and " + duoSurge.Element2 : surgeElement,
                surgeElement.Length > 0 ? Elements.Colour(surgeElement) : MenuParts.Faint);

            actionRows[0].text = "Suggested Hand";
            for (int i = 0; i < RunState.SavedHands.Length; i++)
            {
                var saved = PlayerBuild.SavedHand(i);
                actionRows[i + 1].text = saved == null ? $"Hand {i + 1}: nothing kept" : $"Hand {i + 1}: {Summary(saved)}";
            }
            for (int i = 0; i < actionRows.Length; i++) actionRows[i].color = !Choosing && Item == SuggestedItem + i ? MenuParts.Gold : MenuParts.Plain;

            listRoot.gameObject.SetActive(Choosing);
            choiceLine.gameObject.SetActive(Choosing);
            for (int i = 0; i < actionRows.Length; i++) actionRows[i].gameObject.SetActive(!Choosing);
            bool onRow = !Choosing && Item >= SuggestedItem;
            handCursor.gameObject.SetActive(onRow);
            if (onRow) handCursor.anchoredPosition = new Vector2(922f, -562f - (Item - SuggestedItem) * 56f);
            // The box the cursor is on (or being chosen for) is framed in gold and lit from inside, its name gold.
            bool onBox = Item <= SurgeItem;
            rowCursor.gameObject.SetActive(onBox);
            if (onBox)
            {
                var size = Item == SurgeItem ? SurgeSize : BoxSize;
                rowCursor.sizeDelta = size + new Vector2(CursorMargin * 2f, CursorMargin * 2f);
                rowCursor.anchoredPosition = BoxAt[Item] + new Vector2(-CursorMargin, CursorMargin);
                // Drawn just behind the box it frames, so the surge's box still lies over the slots'.
                int boxAt = boxes[Item].Rect.GetSiblingIndex();
                rowCursor.SetSiblingIndex(rowCursor.GetSiblingIndex() < boxAt ? boxAt - 1 : boxAt);
            }
            for (int i = 0; i <= SurgeItem; i++)
            {
                boxes[i].Back.color = onBox && Item == i ? ChosenInk : MenuParts.Ink;
                boxes[i].Name.color = onBox && Item == i ? MenuParts.Gold : MenuParts.Plain;
            }
            if (!Choosing) ShowItem(loadout, open);
            else ShowList();
            ShowHint();
        }

        private void AppendElement(string element)
        {
            var colour = Elements.Colour(element);
            words.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(colour)).Append('>').Append(element).Append("</color>");
        }

        private static void Dress(in SlotBox box, string name, string line, Color colour)
        {
            box.Name.text = name;
            box.Line.text = line;
            box.Rim.color = colour;
        }

        // What the cursor is on, said on the right.
        private void ShowItem(Loadout loadout, HandOpen open)
        {
            words.Clear();
            if (Item < SurgeItem)
            {
                string id = loadout.SlotAt(Item);
                infoTitle.text = $"{SlotWords[Item]}: {(open.SlotOpen(Item) ? id.Length == 0 ? "empty" : NameOf(id) : "shut")}";
                if (!open.SlotOpen(Item))
                    words.Append(Item == 2 ? "This slot opens when the Champion of the Gate falls" : "This slot opens when the Fire god falls")
                        .Append(", and stays open from then on. In the playtest every slot is open.");
                else
                {
                    if (id.Length > 0) Describe(id);
                    words.Append("\n\n").Append(ControlNames.Of("Interact")).Append(": choose what goes here.");
                }
            }
            else if (Item == SurgeItem)
            {
                string element = HandRules.ElementOfSurge(loadout.Surge);
                var kit = element.Length > 0 ? ElementKit.ByName(element, false) : null;
                var duo = Upgrade.ById(loadout.Surge);
                infoTitle.text = "Surge: " + (DuoSurge.Is(duo) ? duo.Name : kit != null ? kit.SurgeName : "none");
                if (DuoSurge.Is(duo))
                {
                    words.Append("A duo surge of ").Append(duo.Element).Append(" and ").Append(duo.Element2).Append(", lit with ").Append(ControlNames.Of("Surge")).Append('.');
                    if (Details) words.Append("\n\n").Append(Mathf.RoundToInt(duo.Surge.Mana)).Append(" mana, ").Append(Mathf.RoundToInt(duo.Surge.Seconds)).Append(" s. ").Append(duo.Description);
                }
                else if (kit != null)
                {
                    // The surge's own binding names the press for the device in hand ("F" on the keys, "LT tap" on a pad).
                    words.Append(element).Append("'s surge, lit with ").Append(ControlNames.Of("Surge")).Append('.');
                    if (Details) words.Append("\n\n").Append(kit.SurgeCost > 0f ? Mathf.RoundToInt(kit.SurgeCost) + " mana" : "Paid in health").Append(", ").Append(Mathf.RoundToInt(kit.SurgeSeconds)).Append(" s.");
                }
                words.Append("\n\n").Append(ControlNames.Of("Interact")).Append(": choose the surge.");
            }
            else if (Item == SuggestedItem)
            {
                infoTitle.text = "Suggested Hand";
                words.Append("Each carried element's own spell, then the spells learned, in the chart's order: ").Append(Summary(build.Suggested())).Append('.');
                words.Append("\n\n").Append(ControlNames.Of("Interact")).Append(": wear it.");
            }
            else
            {
                int index = Item - FirstSavedItem;
                var saved = PlayerBuild.SavedHand(index);
                infoTitle.text = $"Saved Hand {index + 1}";
                if (saved == null) words.Append("Nothing kept here yet.");
                else words.Append(Summary(saved)).Append(". Spells of an element not carried now are left out when it is worn.");
                words.Append("\n\n");
                if (saved != null) words.Append(ControlNames.Of("Interact")).Append(": wear it.     ");
                words.Append(ControlNames.Of("HeavyAttack")).Append(": keep the Hand worn now here.");
            }
            infoBody.text = words.ToString();
        }

        // The list of choices, scrolled to keep the chosen one (and its heading) in view, and the chosen one's words below.
        private void ShowList()
        {
            infoTitle.text = Item == SurgeItem ? "Choose the surge" : $"Choose for {SlotWords[Item]} ({ControlNames.Of(LayeredButtons.SlotActions[Item])})";
            int shownAt = listEntry.IndexOf(Choice);
            if (shownAt - 1 < firstRow) firstRow = Mathf.Max(0, shownAt - 1);
            if (shownAt >= firstRow + VisibleRows) firstRow = shownAt - VisibleRows + 1;
            for (int i = 0; i < listRows.Count; i++)
            {
                int index = firstRow + i;
                bool used = index < listEntry.Count;
                listRows[i].enabled = used;
                if (!used) continue;
                if (listEntry[index] < 0)
                {
                    listRows[i].text = listLabels[index];
                    listRows[i].color = MenuParts.Gold;
                    listRows[i].fontSize = 18;
                    continue;
                }
                var choice = choices[listEntry[index]];
                string role = choice.Role != SpellRole.None ? "   <color=#B8B0A0>" + choice.Role + "</color>" : "";
                string where = choice.InSlot >= 0 && choice.InSlot != Item ? "   <color=#8C8C8C>(in " + SlotWords[choice.InSlot] + ")</color>" : "";
                listRows[i].text = "   " + choice.Name + role + where;
                listRows[i].color = index == shownAt ? MenuParts.Gold : MenuParts.Plain;
                listRows[i].fontSize = 21;
            }
            listCursor.anchoredPosition = new Vector2(10f, ListTop - (shownAt - firstRow) * RowHeight);
            // The chosen one's line, with its numbers when the details are on.
            words.Clear();
            if (choices.Count > 0)
            {
                var chosen = choices[Choice];
                words.Append(chosen.Line);
                if (Details && chosen.Id.Length > 0)
                {
                    if (Item == SurgeItem) { }
                    else if (chosen.Mana > 0f) words.Append("   ").Append(Mathf.RoundToInt(chosen.Mana)).Append(" mana.");
                    else if (chosen.Cooldown > 0f) words.Append("   Ready again after ").Append(Mathf.RoundToInt(chosen.Cooldown)).Append(" s.");
                    var mark = HandRules.ElementOfBase(chosen.Id).Length == 0 ? Upgrade.ById(chosen.Id) : null;
                    if (mark != null && !string.IsNullOrEmpty(mark.Description)) words.Append("\n").Append(mark.Description);
                }
            }
            infoBody.text = "";
            choiceLine.text = words.ToString();
        }

        private void ShowHint()
        {
            hintVersion = ControlDevices.Version;
            string choose = ControlDevices.Current == ControlDevice.Keys ? "W S, the wheel or the mouse" : "Stick or d-pad";
            string use = ControlNames.Of("Interact"), back = ControlNames.Of("Dodge"), details = ControlNames.Of("WeaponSkill");
            hint.text = Choosing
                ? $"{choose}: choose     {use} or a click: wear it     {details}: {(Details ? "fewer" : "the")} numbers     {back}: back"
                : $"{choose}: choose     {use} or a click: open     {details}: {(Details ? "fewer" : "the")} numbers     {back}: back to the fire";
        }

        // ---- Words for ids.

        private static string NameOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            string baseOf = HandRules.ElementOfBase(id);
            if (baseOf.Length > 0) return ElementKit.ByName(baseOf, false).SpellName;
            var mark = Upgrade.ById(id);
            return mark != null ? mark.Name : id;
        }

        private static string ElementOfSpell(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            string baseOf = HandRules.ElementOfBase(id);
            if (baseOf.Length > 0) return baseOf;
            var mark = Upgrade.ById(id);
            return mark != null ? mark.Element : "";
        }

        private static string RoleOf(string id)
        {
            var timing = Clover.Player.Magic.SpellTimings.For(id);
            return timing.Role != SpellRole.None ? timing.Role.ToString() : "";
        }

        private void Describe(string id)
        {
            string element = ElementOfSpell(id);
            string role = RoleOf(id);
            words.Append(element);
            if (role.Length > 0) words.Append(", ").Append(role);
            words.Append(". ");
            var mark = HandRules.ElementOfBase(id).Length == 0 ? Upgrade.ById(id) : null;
            if (mark != null) words.Append(Details ? mark.Description : HandRules.FirstSentence(mark.Description));
            else words.Append(element).Append("'s own spell.");
            if (Details)
            {
                var timing = Clover.Player.Magic.SpellTimings.For(id);
                float mana = mark != null && mark.Spell != null ? mark.Spell.Cost : timing.Mana;
                float wait = mark != null && mark.Spell != null && mark.Spell.Cooldown > 0f ? mark.Spell.Cooldown : timing.Cooldown;
                if (mana > 0f) words.Append("\n").Append(Mathf.RoundToInt(mana)).Append(" mana.");
                else words.Append("\nReady again after ").Append(Mathf.RoundToInt(wait)).Append(" s.");
            }
        }

        private static string Summary(Loadout hand)
        {
            var text = new StringBuilder(96);
            for (int i = 0; i < Loadout.SlotCount; i++)
            {
                if (i > 0) text.Append(", ");
                string id = hand.SlotAt(i);
                text.Append(id.Length == 0 ? "empty" : NameOf(id));
            }
            string surge = HandRules.ElementOfSurge(hand.Surge);
            if (surge.Length > 0) text.Append("; ").Append(ElementKit.ByName(surge, false).SurgeName);
            return text.ToString();
        }

        // ---- The buttons.

        private void Update()
        {
            if (!IsOpen) return;
            if (Time.frameCount == openedOnFrame || moveAction == null || interactAction == null || dodgeAction == null) return;
            if (GuidePage.BusyThisFrame || ReadingBox.BusyThisFrame) return;
            if (build.Version != shownVersion || ControlDevices.Version != hintVersion) Redraw();
            if (dodgeAction.WasPressedThisFrame() || (pauseAction != null && pauseAction.WasPressedThisFrame()) || (alwaysBack != null && alwaysBack.WasPressedThisFrame()))
            {
                Back();
                return;
            }
            if (interactAction.WasPressedThisFrame())
            {
                Use();
                return;
            }
            if (detailsAction != null && detailsAction.WasPressedThisFrame()) ToggleDetails();
            var mouse = Mouse.current;
            bool mouseHeavy = mouse != null && mouse.rightButton.wasPressedThisFrame;
            if ((heavyAction != null && heavyAction.WasPressedThisFrame()) || mouseHeavy)
            {
                // A right click on a saved Hand's row chooses it first.
                if (mouseHeavy) Pick(mouse.position.ReadValue(), false);
                Keep();
                return;
            }
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel != 0f) Move(wheel > 0f ? -1 : 1);
                if (mouse.leftButton.wasPressedThisFrame && Pick(mouse.position.ReadValue(), true)) return;
            }
            // Down (or right) to the next, up (or left) to the one before: round the diamond, the surge and the Hands in turn.
            var stick = moveAction.ReadValue<Vector2>();
            float along = Mathf.Abs(stick.y) >= Mathf.Abs(stick.x) ? -stick.y : stick.x;
            int direction = along > 0.5f ? 1 : along < -0.5f ? -1 : 0;
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

        // A click on a box, a Hand's row or a choice chooses it, and with use, acts on it. True when it landed on one.
        private bool Pick(Vector2 at, bool use)
        {
            if (Choosing)
            {
                for (int i = 0; i < listRows.Count; i++)
                {
                    int index = firstRow + i;
                    if (!listRows[i].enabled || index >= listEntry.Count || listEntry[index] < 0 || !MenuParts.Contains(listRows[i].rectTransform, at)) continue;
                    Choice = listEntry[index];
                    if (use) Use();
                    else Redraw();
                    return true;
                }
                return false;
            }
            // The surge's box lies over the middle of the diamond, so it is asked first.
            for (int i = SurgeItem; i >= 0; i--)
            {
                if (!MenuParts.Contains(boxes[i].Rect, at)) continue;
                Item = i;
                if (use) Use();
                else Redraw();
                return true;
            }
            for (int i = 0; i < actionRows.Length; i++)
            {
                if (!MenuParts.Contains(actionRows[i].rectTransform, at)) continue;
                Item = SuggestedItem + i;
                if (use) Use();
                else Redraw();
                return true;
            }
            return false;
        }
    }
}
