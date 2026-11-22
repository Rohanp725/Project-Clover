using System;
using System.Collections.Generic;
using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The rest fire's Loadout in UI Toolkit (ui_rest_fire_plan.md 3.5): draws a <see cref="LoadoutController"/> into
    /// Loadout.uxml and feeds it the pad, the keys (<see cref="MenuInput"/>) and the pointer. It sits on the Menus
    /// object's UIDocument for this screen (the editor step's builder puts it there, on the shared MenuPanel); it shows
    /// and hides by its root's display, never by disabling the object, which would rebuild the tree (5.0). While open the
    /// world is stopped, the player held, the HUD hidden and the world behind dimmed.
    ///
    /// The rest menu opens it with <see cref="Ensure"/> then <see cref="Open"/> (the hook point: RestMenu.Act's Hand
    /// entry), and waits on <see cref="Showing"/> and <see cref="BusyThisFrame"/> as it waits on the old Hand.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class LoadoutScreen : MonoBehaviour
    {
        public static LoadoutScreen Instance { get; private set; }
        public static bool Showing => Instance != null && Instance.IsOpen;
        /// <summary>True while open or closed this frame: the rest menu under it leaves its buttons alone then.</summary>
        public static bool BusyThisFrame => Instance != null && (Instance.IsOpen || Instance.closedOnFrame == Time.frameCount);

        // Domain reload is off: the screen of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>Raised for every menu sound (4.5); the audio hook-up plays the UI mixer group's ticks from it.</summary>
        public event Action<MenuSound> Sounded;
        /// <summary>A on the element header: the Reactions page (Q4, step U5) opens from here once built.</summary>
        public event Action ReactionsAsked;

        public bool IsOpen { get; private set; }
        public LoadoutController Controller { get; private set; }
        public int Opened { get; private set; }

        private PlayerController player;
        private InputActionAsset controls;
        private MenuInput input;
        private WeaponStage stage;
        private Canvas hiddenHud;
        private float savedScale = 1f;
        private bool savedSuspended;
        private int openedOnFrame = -1, closedOnFrame = -1;
        private int drawnDevices = -1;
        private VisualElement boundRoot;
        // True while the mouse holds X's glyph on a saved Hand: the hold fills as a held X does.
        private bool pointerKeeping;

        // The tree.
        private VisualElement root, paneLines, listTitle, listColumns, aspectCards, footerLeft, footerRight, paneMore, weaponPicture;
        private ScrollView paneScroll;
        private readonly VisualElement[] places = new VisualElement[LoadoutPlaces.Count];
        private readonly List<ScrollView> listScrolls = new List<ScrollView>();

        /// <summary>The Loadout in this scene (on the Menus object), or null when the scene has none yet.</summary>
        public static LoadoutScreen Ensure()
        {
            if (Instance != null) return Instance;
            Instance = FindAnyObjectByType<LoadoutScreen>(FindObjectsInactive.Include);
            return Instance;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement?.Q("loadout");
            if (root == null) return;
            Bind();
            root.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
            if (IsOpen) Draw();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            // A scene change with the Loadout open must not leave the world stopped.
            if (IsOpen) Close();
            if (stage != null) Destroy(stage.gameObject);
        }

        // ---- Opening and closing.

        /// <summary>Opens the Loadout (only ever from a rest fire). False when there is no player build or no tree.</summary>
        public bool Open()
        {
            if (root == null) OnEnable();
            player = FindAnyObjectByType<PlayerController>();
            var source = GameLoadoutSource.For(player);
            if (root == null || source == null) return false;
            controls = player.Controls;
            input = new MenuInput(controls);
            if (Controller != null) Unhook(Controller);
            Controller = new LoadoutController(source, TextSizeSetting.Current);
            Controller.Changed += Draw;
            Controller.Sounded += OnSound;
            Controller.ReactionsAsked += OnReactions;
            Controller.TextSizeChanged += OnTextSize;
            Controller.PaneScrolled += OnPaneScrolled;
            if (!IsOpen)
            {
                savedScale = Time.timeScale;
                savedSuspended = player.Suspended;
            }
            IsOpen = true;
            Opened++;
            openedOnFrame = Time.frameCount;
            player.Suspended = true;
            Time.timeScale = 0f;
            HideHud(true);
            ShowWeapon();
            input.Enable();
            root.style.display = DisplayStyle.Flex;
            Controller.Open();
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            closedOnFrame = Time.frameCount;
            Controller?.Close();
            input?.Disable();
            if (root != null) root.style.display = DisplayStyle.None;
            HideHud(false);
            if (stage != null) stage.gameObject.SetActive(false);
            // Back to what the rest menu had: the world stays stopped under it.
            Time.timeScale = savedScale;
            if (player != null) player.Suspended = savedSuspended;
        }

        private void Unhook(LoadoutController controller)
        {
            controller.Changed -= Draw;
            controller.Sounded -= OnSound;
            controller.ReactionsAsked -= OnReactions;
            controller.TextSizeChanged -= OnTextSize;
            controller.PaneScrolled -= OnPaneScrolled;
        }

        private void OnSound(MenuSound sound) => Sounded?.Invoke(sound);
        private void OnReactions() => ReactionsAsked?.Invoke();
        private void OnTextSize(int size) => TextSizeSetting.Current = size;

        private void OnPaneScrolled(float lines)
        {
            if (paneScroll == null) return;
            var offset = paneScroll.scrollOffset;
            offset.y = Mathf.Max(0f, offset.y + lines * 40f);
            paneScroll.scrollOffset = offset;
        }

        // The HUD's bars, spell diamond and floor words are hidden while a full-screen menu is open (4.6).
        private void HideHud(bool hide)
        {
            if (hide)
            {
                var hud = MenuParts.FindHud();
                hiddenHud = hud != null ? hud.GetComponentInParent<Canvas>() : null;
                if (hiddenHud != null && !hiddenHud.enabled) hiddenHud = null;
                if (hiddenHud != null) hiddenHud.enabled = false;
            }
            else if (hiddenHud != null)
            {
                hiddenHud.enabled = true;
                hiddenHud = null;
            }
        }

        private void ShowWeapon()
        {
            if (stage == null) stage = WeaponStage.Make();
            stage.gameObject.SetActive(true);
            var rack = player != null ? player.GetComponentInChildren<WeaponRack>() : null;
            stage.Show(rack != null ? rack.ShownPart : null, "");
            if (weaponPicture != null) weaponPicture.style.backgroundImage = Background.FromRenderTexture(stage.Texture);
        }

        // ---- The buttons.

        private void Update()
        {
            if (!IsOpen || Controller == null) return;
            if (ControlDevices.Version != drawnDevices) Draw();
            if (pointerKeeping) Controller.KeepHeld(Time.unscaledDeltaTime);
            if (Time.frameCount != openedOnFrame && input != null && input.Drive(Controller, Time.unscaledDeltaTime))
            {
                Close();
                return;
            }
            DrawKeepRing();
            DrawPaneMore();
            if (stage != null && stage.Look != Controller.PreviewLook) stage.SetLook(Controller.PreviewLook);
        }

        // ---- Binding the tree once.

        private void Bind()
        {
            // Once per tree: a second OnEnable on the same tree must not register every callback twice.
            if (boundRoot == root) return;
            boundRoot = root;
            for (int i = 0; i < LoadoutPlaces.Count; i++)
            {
                var place = root.Q(LoadoutPlaces.Names[i]);
                places[i] = place;
                if (place == null) continue;
                var at = (LoadoutPlace)i;
                // Focus follows the pointer only when it moves, so parts redrawn under a resting pointer never take it.
                place.RegisterCallback<PointerMoveEvent>(_ => Controller?.PointAt(at));
                place.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.button == 0) Controller?.Click(at);
                });
            }
            // X on a saved Hand: a click on its glyph keeps the Hand worn there (a held click for one that holds a Hand).
            foreach (var hand in new[] { LoadoutPlace.Hand1, LoadoutPlace.Hand2, LoadoutPlace.Hand3 })
            {
                var keep = places[(int)hand]?.Q(className: "hand-keep");
                if (keep == null) continue;
                keep.RegisterCallback<PointerDownEvent>(evt =>
                {
                    evt.StopPropagation();
                    if (evt.button != 0 || Controller == null) return;
                    Controller.KeepDown();
                    pointerKeeping = LoadoutPlaces.HandOf(Controller.Focus) > 0 && !Controller.Board.Hands[LoadoutPlaces.HandOf(Controller.Focus)].Empty;
                });
                keep.RegisterCallback<PointerUpEvent>(_ => StopKeeping());
                keep.RegisterCallback<PointerLeaveEvent>(_ => StopKeeping());
                // The click that ends a hold must not also wear the Hand under it.
                keep.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            }
            // Y on the focused slot: a click on its glyph swaps the shape.
            foreach (var slot in new[] { LoadoutPlace.West, LoadoutPlace.North, LoadoutPlace.East, LoadoutPlace.South })
                places[(int)slot]?.Q(className: "slot-glyph")?.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    Controller?.Shape();
                });
            // A right click is B: MenuInput reads it from the UI map, so it is not handled here as well.
            // UI Toolkit's own navigation is swallowed: MenuInput moves focus at the game's own repeat.
            root.RegisterCallback<NavigationMoveEvent>(Swallow, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(Swallow, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationCancelEvent>(Swallow, TrickleDown.TrickleDown);

            paneScroll = root.Q<ScrollView>("pane-scroll");
            paneLines = root.Q("pane-lines");
            paneMore = root.Q("pane-more");
            listTitle = root.Q("list-title");
            listColumns = root.Q("list-columns");
            aspectCards = root.Q("aspect-cards");
            footerLeft = root.Q("footer-left");
            footerRight = root.Q("footer-right");
            weaponPicture = root.Q("weapon-picture");

            // Every .icon box gets its painted shape; the five Temper pips and the five role cells are made once.
            root.Query(className: "icon").ForEach(box =>
            {
                if (box.Q<PaintedIcon>() == null) box.Add(new PaintedIcon());
            });
            SetIcon(root.Q("wallet-memories-icon"), IconShapes.Mote);
            SetIcon(root.Q("wallet-shards-icon"), IconShapes.Shard);
            SetIcon(root.Q("technique-lock"), IconShapes.Lock);
            var pips = root.Q("temper-pips");
            if (pips != null && pips.childCount == 0)
                for (int i = 0; i < LoadoutBoard.TemperMost; i++) pips.Add(Part("pip"));
            var cells = root.Q("role-cells");
            if (cells != null && cells.childCount == 0)
                foreach (var role in LoadoutWords.Roles)
                {
                    var cell = Part("role-cell");
                    cell.Add(Icon(IconShapes.ForRole(role), "icon-32"));
                    cell.Add(new Label { name = "count" }.WithClass("role-count"));
                    cell.Add(new Label { name = "was" }.WithClass("role-was"));
                    cells.Add(cell);
                }
        }

        private void StopKeeping()
        {
            if (!pointerKeeping) return;
            pointerKeeping = false;
            Controller?.KeepUp();
        }

        private void Swallow(EventBase evt)
        {
            evt.StopPropagation();
            root?.panel?.focusController?.IgnoreEvent(evt);
        }

        // ---- Drawing.

        private void Draw()
        {
            if (root == null || Controller == null) return;
            var loadout = Controller;
            var board = loadout.Board;
            if (drawnDevices != ControlDevices.Version) root.Q("pane-more-glyph")?.Clear();
            drawnDevices = ControlDevices.Version;
            var device = ControlDevices.Current;

            // The root's classes: the text size, the folded half, what is open.
            foreach (int size in TextSizes.Steps) root.EnableInClassList(TextSizes.ClassOf(size), size == loadout.TextSize && size != TextSizes.Normal);
            root.EnableInClassList("fold-steel", loadout.Folded == LoadoutHalf.Steel);
            root.EnableInClassList("fold-element", loadout.Folded == LoadoutHalf.Element);
            root.EnableInClassList("mode-list", loadout.Mode == LoadoutMode.SpellList);
            root.EnableInClassList("mode-cards", loadout.Mode == LoadoutMode.AspectCards);

            Text("wallet-memories", LoadoutWords.Memories(board.Memories));
            Text("wallet-shards", board.Shards + " Shards");

            DrawSteel(board);
            DrawElement(board, device);
            DrawFocus();
            DrawList(device);
            DrawAspectCards();
            DrawPane(device);
            DrawFooter(device);
        }

        private void DrawSteel(LoadoutBoard board)
        {
            var loadout = Controller;
            Text("weapon-name", board.Weapon.Length > 0 ? board.Weapon : "No weapon");
            var aspect = places[(int)LoadoutPlace.Aspect];
            Card(aspect, board.Aspect.Id.Length > 0 ? board.Aspect.Icon : IconShapes.Blade, board.Aspect.Id.Length > 0 ? board.Aspect.Name : "No aspect",
                board.Aspect.MarksTotal > 0 ? board.Aspect.MarksBought + " of " + board.Aspect.MarksTotal + " marks bought" : "Buy one on the Way chart", board.Aspect.Id.Length == 0);
            var keystones = loadout.PreviewKeystones;
            Pair(places[(int)LoadoutPlace.Keystone1], keystones[0], "No keystone");
            Pair(places[(int)LoadoutPlace.Keystone2], keystones[1], "");
            Pair(places[(int)LoadoutPlace.Bearing1], board.Bearings[0], "No bearing");
            Pair(places[(int)LoadoutPlace.Bearing2], board.Bearings[1], "");

            var temper = places[(int)LoadoutPlace.Temper];
            temper?.Q<Label>(className: "card-name")?.SetText(LoadoutWords.TemperName(board.Temper));
            var pips = root.Q("temper-pips");
            for (int i = 0; pips != null && i < pips.childCount; i++) pips[i].EnableInClassList("on", i < board.Temper);

            var technique = places[(int)LoadoutPlace.Technique];
            technique?.Q<Label>(className: "card-name")?.SetText(board.Technique.Id.Length > 0 ? board.Technique.Name : "No technique");
            var seal = root.Q("technique-seal");
            if (seal != null)
            {
                Show(seal, board.Technique.Id.Length > 0 && !board.Technique.Bought);
                seal.Q<Label>()?.SetText(board.Technique.Lock.StartsWith("Sealed", StringComparison.Ordinal) ? "Sealed" : "Not bought");
            }

            // The infused strikes: one line, never wrapped.
            var line = root.Q("infused-line");
            if (line != null)
            {
                line.Clear();
                if (board.StrikeFirst.Length == 0) line.Add(new Label("Carry an element to infuse your strikes").WithClass("t-meta"));
                else
                {
                    Strike(line, "LightAttack", board.StrikeFirst, false);
                    if (board.StrikeSecond.Length > 0)
                    {
                        line.Add(new Label("·").WithClass("t-meta", "gap-left"));
                        Strike(line, "HeavyAttack", board.StrikeSecond, board.StrikeSecondDeep);
                    }
                }
            }

            Text("steel-summary", loadout.SteelSummary());
            SetIcon(root.Q("steel-folded-icon"), board.Aspect.Id.Length > 0 ? board.Aspect.Icon : IconShapes.Blade);
        }

        private void Strike(VisualElement line, string attack, string element, bool deep)
        {
            var device = ControlDevices.Current;
            AddGlyph(line, "SpellLayer", device, true, line.childCount > 0);
            line.Add(new Label("+").WithClass("t-meta", "gap-left"));
            AddGlyph(line, attack, device, true, true);
            line.Add(new Label((deep ? "Deep " : "") + element + " strike").WithClass("t-row", "t-bold", "t-nowrap", "gap-left", "text-" + Key(element)));
        }

        private void DrawElement(LoadoutBoard board, ControlDevice device)
        {
            var loadout = Controller;
            var first = root.Q<Label>("element-first");
            var plus = root.Q<Label>("element-plus");
            var second = root.Q<Label>("element-second");
            bool two = board.Second.Length > 0 && !board.AllIn;
            Tint(first, "text-", board.First);
            first?.SetText(board.First.Length > 0 ? board.First : "None");
            Show(plus, two);
            Show(second, two);
            Tint(second, "text-", board.Second);
            second?.SetText(board.Second);
            Text("element-carried", board.AllIn ? "All in, carried from the hub" : "Carried from the hub");
            var devotion = root.Q<Label>("element-devotion");
            Show(devotion, board.Devotion.Length > 0);
            devotion?.SetText("Devotion: " + board.Devotion);

            // The role strip, from the preview: a changed count is outlined with its old number beside it.
            Text("roles-missing", loadout.MissingRoles);
            var cells = root.Q("role-cells");
            for (int i = 0; cells != null && i < cells.childCount && i < LoadoutWords.Roles.Length; i++)
            {
                int role = (int)LoadoutWords.Roles[i];
                int now = loadout.RolesPreview[role], was = loadout.Roles[role];
                var cell = cells[i];
                cell.EnableInClassList("missing", now == 0);
                cell.EnableInClassList("changed", now != was);
                cell.Q<Label>("count")?.SetText(now.ToString());
                var wasLabel = cell.Q<Label>("was");
                Show(wasLabel, now != was);
                wasLabel?.SetText("was " + was);
            }

            for (int slot = 0; slot < 4; slot++) Slot(places[(int)LoadoutPlaces.PlaceOfSlot(slot)], board.Slots[slot], slot, device);
            Surge(places[(int)LoadoutPlace.Surge], board.Surge, device);
            for (int hand = 0; hand < board.Hands.Length; hand++) Hand(places[(int)LoadoutPlace.Starter + hand], board.Hands[hand], hand, device);

            Text("element-summary", board.ElementWords);
            Text("element-summary-roles", loadout.RoleLetters());
            var icons = root.Q("element-summary-icons");
            if (icons != null)
            {
                icons.Clear();
                foreach (var spell in board.Slots) icons.Add(Icon(spell.IsEmpty ? "" : IconShapes.ForElement(spell.Element), "icon-inline", "el-" + Key(spell.Element)));
                icons.Add(Icon(board.Surge.IsEmpty ? "" : IconShapes.ForElement(board.Surge.Element), "icon-inline", "el-" + Key(board.Surge.Element)));
            }
        }

        private void Slot(VisualElement card, SpellCard spell, int slot, ControlDevice device)
        {
            if (card == null) return;
            Rim(card, spell.IsEmpty || !spell.Open ? "" : spell.Element);
            card.EnableInClassList("empty", spell.Open && spell.IsEmpty);
            card.EnableInClassList("shut", !spell.Open);
            SetIcon(card.Q(className: "slot-icon"), !spell.Open ? IconShapes.Lock : spell.IsEmpty ? "" : IconShapes.ForElement(spell.Element));
            Tint(card.Q(className: "slot-icon"), "el-", spell.Open ? spell.Element : "");
            card.Q<Label>(className: "slot-name")?.SetText(!spell.Open ? spell.ShutReason : spell.IsEmpty ? "Empty" : spell.Name);
            bool words = spell.Open && !spell.IsEmpty;
            var roleLine = card.Q(className: "slot-role-line");
            Show(roleLine, words);
            SetIcon(card.Q(className: "slot-role-icon"), IconShapes.ForRole(spell.Role));
            string role = LoadoutWords.RoleName(spell.Role);
            string cost = spell.CostShort;
            card.Q<Label>(className: "slot-role")?.SetText(role.Length > 0 && cost.Length > 0 ? role + " · " + cost : role + cost);
            var shape = card.Q<Label>(className: "slot-shape");
            Show(shape, words && spell.ShapeName.Length > 0);
            shape?.SetText("Shape: " + spell.ShapeName);
            // The button that casts it, on the card's outer edge, like the pad.
            var glyph = card.Q<Label>(className: "slot-glyph");
            string path = Glyphs.PathOf(controls, LoadoutWords.SlotActions[slot], device);
            if (glyph != null && path.Length > 0)
            {
                Glyphs.Dress(glyph, path, device);
                glyph.AddToClassList("slot-glyph");
            }
        }

        private void Surge(VisualElement surge, SpellCard spell, ControlDevice device)
        {
            if (surge == null) return;
            Rim(surge.Q("surge-ring"), spell.IsEmpty ? "" : spell.Element);
            SetIcon(surge.Q(className: "slot-icon"), !spell.Open ? IconShapes.Lock : spell.IsEmpty ? "" : IconShapes.ForElement(spell.Element));
            Tint(surge.Q(className: "slot-icon"), "el-", spell.Element);
            surge.Q<Label>(className: "surge-name")?.SetText(!spell.Open ? spell.ShutReason : spell.IsEmpty ? "No surge" : spell.Name);
            var glyph = surge.Q<Label>(className: "surge-glyph");
            string path = Glyphs.PathOf(controls, "Surge", device);
            if (glyph != null && path.Length > 0)
            {
                Glyphs.Dress(glyph, path, device, true);
                glyph.AddToClassList("surge-glyph");
            }
        }

        private void Hand(VisualElement card, HandCard hand, int index, ControlDevice device)
        {
            if (card == null) return;
            var minis = card.Q(className: "hand-minis");
            if (minis != null)
            {
                minis.Clear();
                Show(minis, !hand.Empty);
                if (!hand.Empty)
                {
                    foreach (var spell in hand.Slots) minis.Add(Icon(spell.IsEmpty ? "" : IconShapes.ForElement(spell.Element), "icon-inline", "el-" + Key(spell.Element)));
                    minis.Add(Part("hand-sep"));
                    minis.Add(Icon(hand.Surge.IsEmpty ? "" : IconShapes.ForElement(hand.Surge.Element), "icon-inline", "el-" + Key(hand.Surge.Element)));
                }
            }
            Show(card.Q(className: "hand-empty"), hand.Empty);
            Show(card.Q(className: "hand-tag"), hand.Worn && !hand.Empty);
            var keep = card.Q<Label>(className: "hand-keep");
            if (keep != null)
            {
                bool here = Controller.Mode == LoadoutMode.Board && Controller.Focus == LoadoutPlace.Starter + index;
                Show(keep, here);
                string path = Glyphs.PathOf(controls, "Keep", device);
                if (path.Length > 0)
                {
                    var ring = keep.Q(className: "hold-ring");
                    Glyphs.Dress(keep, path, device, true);
                    keep.AddToClassList("hand-keep");
                    if (ring != null && ring.parent != keep) keep.Add(ring);
                }
            }
        }

        private void DrawKeepRing()
        {
            int hand = LoadoutPlaces.HandOf(Controller.Focus);
            if (hand < 1) return;
            var ring = places[(int)Controller.Focus]?.Q(className: "hold-ring");
            if (ring != null) ring.style.opacity = Controller.KeepProgress;
        }

        private void DrawFocus()
        {
            var loadout = Controller;
            for (int i = 0; i < places.Length; i++)
            {
                if (places[i] == null) continue;
                places[i].EnableInClassList("is-focus", loadout.Mode == LoadoutMode.Board && (int)loadout.Focus == i);
                places[i].EnableInClassList("is-choosing", (loadout.Mode == LoadoutMode.SpellList && loadout.ListSlot >= 0 && (int)LoadoutPlaces.PlaceOfSlot(loadout.ListSlot) == i)
                    || (loadout.Mode == LoadoutMode.AspectCards && i == (int)LoadoutPlace.Aspect));
            }
            if (loadout.Mode == LoadoutMode.Board) places[(int)loadout.Focus]?.Focus();
        }

        // ---- The spell list (3.5.5): a column per element under its coloured header; rows with their role, price and shape.

        private void DrawList(ControlDevice device)
        {
            var loadout = Controller;
            var list = root.Q("spell-list");
            Show(list, loadout.Mode == LoadoutMode.SpellList);
            if (loadout.Mode != LoadoutMode.SpellList || listColumns == null) return;
            listTitle.Clear();
            AddWords(listTitle, loadout.ListTitle, "t-name-small", device);
            listColumns.Clear();
            listScrolls.Clear();
            VisualElement focusedRow = null;
            for (int c = 0; c < loadout.Columns.Count; c++)
            {
                var column = loadout.Columns[c];
                var box = Part("list-column");
                box.Add(new Label(column.Element.Length > 0 ? column.Element : "Spells").WithClass("t-label", "list-column-header", "text-" + Key(column.Element)));
                var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
                scroll.AddToClassList("list-scroll");
                for (int r = 0; r < column.Rows.Count; r++)
                {
                    var row = Row(column.Rows[r], c, r, device);
                    if (c == loadout.Column && r == loadout.Row) focusedRow = row;
                    scroll.Add(row);
                }
                box.Add(scroll);
                listScrolls.Add(scroll);
                listColumns.Add(box);
            }
            if (focusedRow != null && loadout.Column < listScrolls.Count)
            {
                var scroll = listScrolls[loadout.Column];
                focusedRow.schedule.Execute(() => scroll.ScrollTo(focusedRow));
            }
        }

        private VisualElement Row(SpellCard spell, int column, int index, ControlDevice device)
        {
            var loadout = Controller;
            var row = Part("list-row", "place");
            bool focused = column == loadout.Column && index == loadout.Row;
            row.EnableInClassList("is-focus", focused);
            row.Add(Icon(spell.IsEmpty ? "" : IconShapes.ForElement(spell.Element), "icon-list", "el-" + Key(spell.Element)));
            var words = Part("list-row-words", "column");
            words.Add(new Label(spell.Name).WithClass("t-row", "t-bold", spell.IsEmpty ? "t-off" : "t-plain"));
            if (!spell.IsEmpty)
            {
                string meta = LoadoutWords.RoleName(spell.Role);
                if (spell.CostShort.Length > 0) meta += (meta.Length > 0 ? " · " : "") + spell.CostShort;
                int shape = loadout.ShapeOf(spell);
                if (spell.Shapes.Length > 0) meta += " · " + spell.Shapes[Mathf.Clamp(shape, 0, spell.Shapes.Length - 1)];
                var line = Part("row");
                if (spell.Role != SpellRole.None) line.Add(Icon(IconShapes.ForRole(spell.Role), "icon-role", "icon-dim"));
                line.Add(new Label(meta).WithClass("t-small", "t-nowrap"));
                words.Add(line);
            }
            row.Add(words);
            var worn = loadout.ListSlot == 4 ? loadout.Board.Surge : loadout.Board.Slots[loadout.ListSlot];
            if (!spell.IsEmpty && spell.Id == worn.Id) row.Add(new Label("Worn").WithClass("set-tag", "list-tag"));
            else if (spell.InSlot >= 0 && spell.InSlot != loadout.ListSlot)
            {
                var tag = Part("row", "list-tag");
                tag.Add(new Label("In").WithClass("t-small", "t-nowrap"));
                AddGlyph(tag, LoadoutWords.SlotActions[spell.InSlot], device, true, true);
                tag.Add(new Label(LoadoutWords.SlotWords[spell.InSlot]).WithClass("t-small", "t-nowrap", "gap-left"));
                row.Add(tag);
            }
            var frame = Part("focus-frame");
            frame.pickingMode = PickingMode.Ignore;
            row.Add(frame);
            row.RegisterCallback<PointerMoveEvent>(_ => Controller?.PointAtRow(column, index));
            row.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                Controller?.ClickRow(column, index);
            });
            return row;
        }

        // ---- The aspect cards (3.5.5).

        private void DrawAspectCards()
        {
            var loadout = Controller;
            Show(aspectCards, loadout.Mode == LoadoutMode.AspectCards);
            if (loadout.Mode != LoadoutMode.AspectCards || aspectCards == null) return;
            aspectCards.Clear();
            aspectCards.EnableInClassList("four", loadout.AspectCards.Count > 3);
            for (int i = 0; i < loadout.AspectCards.Count; i++)
            {
                var card = loadout.AspectCards[i];
                var box = Part("card", "place", "aspect-choice", "column");
                box.EnableInClassList("is-focus", i == loadout.Card);
                box.EnableInClassList("locked", !card.Bought);
                box.Add(Icon(card.Bought ? card.Icon : IconShapes.Lock, "icon-64", "card-icon"));
                box.Add(new Label(card.Name).WithClass("t-name", card.Bought ? "t-plain" : "t-off"));
                box.Add(new Label(card.Short).WithClass("t-meta"));
                box.Add(new Label(card.Bought ? card.MarksBought + " of " + card.MarksTotal + " marks bought" : card.Lock).WithClass("t-small"));
                if (card.Set) box.Add(new Label("Set").WithClass("set-tag"));
                var frame = Part("focus-frame");
                frame.pickingMode = PickingMode.Ignore;
                box.Add(frame);
                int at = i;
                box.RegisterCallback<PointerMoveEvent>(_ => Controller?.PointAtCard(at));
                box.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    Controller?.ClickCard(at);
                });
                aspectCards.Add(box);
            }
        }

        // ---- The pane (4.4 and 3.5.6).

        private void DrawPane(ControlDevice device)
        {
            var pane = Controller.Pane;
            SetIcon(root.Q("pane-icon"), pane.Icon);
            Tint(root.Q("pane-icon"), "el-", pane.IconElement);
            Text("pane-title", pane.Title);
            Text("pane-kind", pane.Kind);
            var price = root.Q<Label>("pane-price");
            Show(price, pane.Price.Length > 0);
            price?.SetText(pane.Price);
            // A Way mark's price is in Memories; an aspect's "6 of 10 marks bought" is plain meta.
            price?.EnableInClassList("t-memories", pane.Price.StartsWith("Bought", StringComparison.Ordinal));
            if (paneLines == null) return;
            paneLines.Clear();
            var section = (PaneSection)(-1);
            bool actionShown = false;
            foreach (var line in pane.Lines)
            {
                if (line.Section == PaneSection.Foot && !actionShown)
                {
                    actionShown = true;
                    Action(pane, device);
                }
                if (line.Section != section)
                {
                    bool rule = line.Section == PaneSection.Compare || (line.Section == PaneSection.Foot);
                    if (rule && paneLines.childCount > 0) paneLines.Add(Part("rule"));
                    else if (section != (PaneSection)(-1) && line.Section == PaneSection.Facts) paneLines.Add(Part("pane-section"));
                    section = line.Section;
                }
                var row = Part("pane-line");
                string size = line.Section == PaneSection.Body ? "t-body" : line.Section == PaneSection.Compare ? "t-row" : line.Section == PaneSection.Foot ? "t-small" : "t-meta";
                string tone = line.Tone switch
                {
                    PaneTone.Loss => "t-loss",
                    PaneTone.Memories => "t-memories",
                    PaneTone.Keyword => "t-gold",
                    PaneTone.Meta => "",
                    _ => line.Section == PaneSection.Foot ? "" : "t-plain",
                };
                if (line.Lead.Length > 0)
                {
                    row.Add(new Label(line.Lead).WithClass(size, "t-bold", line.Tone == PaneTone.Plain ? "t-plain" : tone));
                    AddWords(row, line.Text, size + (tone.Length > 0 && line.Tone != PaneTone.Loss && line.Tone != PaneTone.Keyword ? " " + tone : ""), device, true);
                }
                else AddWords(row, line.Text, size + (tone.Length > 0 ? " " + tone : ""), device);
                paneLines.Add(row);
            }
            if (!actionShown) Action(pane, device);
        }

        private void Action(LoadoutPane pane, ControlDevice device)
        {
            if (pane.Action.Length == 0 || paneLines == null) return;
            var row = Part("row", "pane-action");
            AddGlyph(row, pane.Action, device, false, false);
            row.Add(new Label(pane.ActionText + (pane.ActionHold ? " (hold)" : "")).WithClass("gap-left"));
            paneLines.Add(row);
        }

        private void DrawPaneMore()
        {
            if (paneScroll == null || paneMore == null) return;
            float overflow = paneScroll.contentContainer.layout.height - paneScroll.contentViewport.layout.height;
            bool more = overflow > 4f && paneScroll.scrollOffset.y < overflow - 4f;
            if (more == (paneMore.resolvedStyle.display == DisplayStyle.Flex)) return;
            Show(paneMore, more);
            var glyph = root.Q("pane-more-glyph");
            if (more && glyph != null && glyph.childCount == 0) AddGlyph(glyph, "ScrollPane", ControlDevices.Current, true, false);
        }

        // ---- The footer: Back on the left; the screen-wide presses on the right (4.5).

        private void DrawFooter(ControlDevice device)
        {
            if (footerLeft == null || footerRight == null) return;
            footerLeft.Clear();
            AddGlyph(footerLeft, "Cancel", device, false, false);
            footerLeft.Add(new Label(Controller.Mode == LoadoutMode.Board ? "Back to the fire" : "Back").WithClass("t-meta", "gap-left"));
            footerRight.Clear();
            AddGlyph(footerRight, "HalfLeft", device, false, false);
            AddGlyph(footerRight, "HalfRight", device, false, true);
            footerRight.Add(new Label(Controller.Mode == LoadoutMode.SpellList ? "Element groups" : "Steel / Element").WithClass("t-meta", "gap-left"));
            AddGlyph(footerRight, "TextSize", device, false, true);
            footerRight[footerRight.childCount - 1].AddToClassList("footer-gap");
            footerRight.Add(new Label(TextSizes.Words(Controller.TextSize)).WithClass("t-meta", "gap-left"));
            AddGlyph(footerRight, "ScrollPane", device, false, true);
            footerRight[footerRight.childCount - 1].AddToClassList("footer-gap");
            footerRight.Add(new Label("Scroll details").WithClass("t-meta", "gap-left"));
        }

        // ---- Small parts.

        // Words with presses in braces ("Press {LightAttack} again"): the presses become glyphs.
        private void AddWords(VisualElement into, string text, string classes, ControlDevice device, bool gapFirst = false)
        {
            if (string.IsNullOrEmpty(text)) return;
            var classList = classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int at = 0;
            bool first = true;
            while (at < text.Length)
            {
                int open = text.IndexOf('{', at);
                string words = open < 0 ? text.Substring(at) : text.Substring(at, open - at);
                if (words.Trim().Length > 0)
                {
                    var label = new Label(words.Trim()).WithClass(classList);
                    if (!first || gapFirst) label.AddToClassList("gap-left");
                    into.Add(label);
                    first = false;
                }
                if (open < 0) break;
                int close = text.IndexOf('}', open);
                if (close < 0) break;
                AddGlyph(into, text.Substring(open + 1, close - open - 1), device, true, !first || gapFirst);
                first = false;
                at = close + 1;
            }
        }

        private void AddGlyph(VisualElement into, string action, ControlDevice device, bool small, bool gap)
        {
            var glyph = Glyphs.Make(controls, action, device, small);
            if (glyph == null)
            {
                // No binding on this device: the action's own name, so nothing is silently missing.
                glyph = new Label(action).WithClass("glyph", "glyph-wide");
            }
            if (gap) glyph.AddToClassList("gap-left");
            into.Add(glyph);
        }

        private void Card(VisualElement card, string icon, string name, string line, bool off)
        {
            if (card == null) return;
            SetIcon(card.Q(className: "card-icon"), icon);
            var nameLabel = card.Q<Label>(className: "card-name");
            nameLabel?.SetText(name);
            nameLabel?.EnableInClassList("t-off", off);
            card.Q<Label>(className: "card-line")?.SetText(line);
        }

        private void Pair(VisualElement cell, WayCard mark, string none)
        {
            if (cell == null) return;
            bool empty = string.IsNullOrEmpty(mark.Id);
            Show(cell, !empty || none.Length > 0);
            Card(cell, empty ? IconShapes.Lock : mark.Bought ? mark.Icon : IconShapes.Lock, empty ? none : mark.Name,
                empty ? "" : mark.Bought ? mark.Short : LoadoutWords.Memories(mark.Price), empty || !mark.Bought);
            var icon = cell.Q(className: "card-icon");
            icon?.EnableInClassList("icon-dim", !mark.Set);
            icon?.EnableInClassList("icon-off", empty || !mark.Bought);
            cell.Q(className: "lamp")?.EnableInClassList("lit", mark.Set);
            Show(cell.Q(className: "set-tag"), mark.Set);
            var line = cell.Q<Label>(className: "card-line");
            line?.EnableInClassList("t-memories", !empty && !mark.Bought);
            Show(line, line != null && line.text.Length > 0);
        }

        private static void Rim(VisualElement element, string elementName)
        {
            if (element == null) return;
            foreach (var each in Elements.All) element.RemoveFromClassList("rim-" + Key(each.Name));
            element.RemoveFromClassList("rim-both");
            if (!string.IsNullOrEmpty(elementName)) element.AddToClassList("rim-" + Key(elementName));
        }

        private static void Tint(VisualElement element, string prefix, string elementName)
        {
            if (element == null) return;
            foreach (var each in Elements.All) element.RemoveFromClassList(prefix + Key(each.Name));
            element.RemoveFromClassList(prefix + "both");
            if (!string.IsNullOrEmpty(elementName)) element.AddToClassList(prefix + Key(elementName));
        }

        private static string Key(string elementName) => string.IsNullOrEmpty(elementName) ? "none" : elementName.ToLowerInvariant();

        private static void SetIcon(VisualElement box, string icon)
        {
            if (box == null) return;
            var painted = box.Q<PaintedIcon>();
            if (painted == null) box.Add(painted = new PaintedIcon());
            painted.Icon = icon ?? "";
        }

        private static VisualElement Icon(string icon, params string[] classes)
        {
            var box = new VisualElement();
            box.AddToClassList("icon");
            foreach (string name in classes) box.AddToClassList(name);
            box.Add(new PaintedIcon(icon));
            return box;
        }

        private static VisualElement Part(params string[] classes)
        {
            var part = new VisualElement();
            foreach (string name in classes) part.AddToClassList(name);
            return part;
        }

        private void Text(string name, string text) => root.Q<Label>(name)?.SetText(text ?? "");

        private static void Show(VisualElement element, bool shown)
        {
            if (element != null) element.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>Small helpers for building the tree in code.</summary>
    public static class ToolkitParts
    {
        public static Label WithClass(this Label label, params string[] classes)
        {
            foreach (string name in classes)
                if (!string.IsNullOrEmpty(name)) label.AddToClassList(name);
            return label;
        }

        public static void SetText(this Label label, string text)
        {
            if (label != null && label.text != text) label.text = text;
        }
    }
}
