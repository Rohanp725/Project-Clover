using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.UI.Toolkit
{
    /// <summary>What the Loadout is doing: the board, a spell list open over the Steel half, or the aspect cards over the Element half.</summary>
    public enum LoadoutMode
    {
        Board,
        SpellList,
        AspectCards,
    }

    /// <summary>Which half is folded to its summary line at 125% and 150% text (3.5.8); None at 100%.</summary>
    public enum LoadoutHalf
    {
        None,
        Steel,
        Element,
    }

    /// <summary>The menu's sounds (4.5): a soft tick on a move, a dull tick where a move or press goes nowhere, one for setting a thing.</summary>
    public enum MenuSound
    {
        Move,
        Blocked,
        Set,
        Open,
        Close,
    }

    /// <summary>
    /// The Loadout's logic (ui_rest_fire_plan.md 3.5), with no screen behind it: where focus is and where the stick takes
    /// it, what each press does, the spell lists and the aspect cards, the preview of a choice (the pane, the role
    /// counts, the weapon's look) and its commit, Y for shapes, the held X that replaces a kept Hand, the halves, the
    /// text size and which half folds. Everything it changes goes through <see cref="ILoadoutSource"/>; nothing changes
    /// until A, and B leaves with nothing changed. <see cref="LoadoutScreen"/> draws it; EditMode tests drive it.
    /// </summary>
    public sealed partial class LoadoutController
    {
        /// <summary>How long X is held to replace a kept Hand (3.5.4).</summary>
        public const float KeepHoldSeconds = 0.6f;

        // The last place changed at any fire: the Loadout opens there (3.5.3). Kept for the play session.
        private static LoadoutPlace lastChanged = LoadoutPlaces.ElementFirst;

        // Domain reload is off: a new play opens on the West slot again.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => lastChanged = LoadoutPlaces.ElementFirst;

        /// <summary>The place the Loadout opens on: the last one changed at any fire, West the first time.</summary>
        public static LoadoutPlace LastChanged => lastChanged;

        /// <summary>Opens on West again, as a new play does (tests start from it).</summary>
        public static void ForgetLastChanged() => lastChanged = LoadoutPlaces.ElementFirst;

        private readonly ILoadoutSource source;
        private readonly FocusGrid grid = LoadoutPlaces.NewGrid();
        private readonly Dictionary<string, int> previewShapes = new Dictionary<string, int>();
        private readonly List<SpellCard> choices = new List<SpellCard>();
        private LoadoutPlace steelLast = LoadoutPlaces.SteelFirst, elementLast = LoadoutPlaces.ElementFirst;
        private bool keepHolding;
        private int keepHand;

        public LoadoutController(ILoadoutSource source, int textSize = 100)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            TextSize = TextSizes.Normalise(textSize);
            Board = new LoadoutBoard();
        }

        public LoadoutBoard Board { get; private set; }
        public LoadoutMode Mode { get; private set; }
        public LoadoutPlace Focus { get; private set; } = LoadoutPlaces.ElementFirst;
        public bool IsOpen { get; private set; }

        /// <summary>The text size, 100, 125 or 150 (3.5.8).</summary>
        public int TextSize { get; private set; }

        /// <summary>Raised after anything the screen shows has changed.</summary>
        public event Action Changed;
        public event Action<MenuSound> Sounded;
        /// <summary>A on the element header: the Reactions page (Q4) is asked for.</summary>
        public event Action ReactionsAsked;
        /// <summary>The text size was stepped; the screen saves it with the settings.</summary>
        public event Action<int> TextSizeChanged;
        /// <summary>The right stick or the page keys asked the pane to scroll, by this many lines (down is positive).</summary>
        public event Action<float> PaneScrolled;

        /// <summary>Bumped on every change, for probes and the screen.</summary>
        public int Version { get; private set; }

        /// <summary>The details pane, describing the focused thing.</summary>
        public LoadoutPane Pane { get; } = new LoadoutPane();

        /// <summary>The role counts of the Hand worn, by <see cref="SpellRole"/>.</summary>
        public int[] Roles { get; } = new int[6];

        /// <summary>The role counts as the focused choice would make them (the same as <see cref="Roles"/> when nothing is previewed).</summary>
        public int[] RolesPreview { get; } = new int[6];

        /// <summary>The role strip's line for what is missing, from the preview.</summary>
        public string MissingRoles { get; private set; } = "";

        /// <summary>The slot being chosen for while a spell list is open (0 West to 3 South, 4 the surge), or -1.</summary>
        public int ListSlot { get; private set; } = -1;

        /// <summary>The open spell list's columns: one per carried element, then "Both" for duo spells.</summary>
        public List<SpellColumn> Columns { get; } = new List<SpellColumn>();
        public int Column { get; private set; }
        public int Row { get; private set; }

        /// <summary>The aspect cards while they are open.</summary>
        public List<WayCard> AspectCards { get; } = new List<WayCard>();
        public int Card { get; private set; }

        /// <summary>How far a hold of X has filled its ring (0 to 1).</summary>
        public float KeepProgress { get; private set; }

        /// <summary>A line for the pane's foot after a press that could not act (Y on a spell with one shape); cleared on the next move.</summary>
        public string Notice { get; private set; } = "";

        /// <summary>The focused row of the open spell list, or null.</summary>
        public SpellCard FocusedRow => Mode == LoadoutMode.SpellList && Column < Columns.Count && Row < Columns[Column].Rows.Count ? Columns[Column].Rows[Row] : null;

        /// <summary>The focused aspect card, or null.</summary>
        public WayCard FocusedCard => Mode == LoadoutMode.AspectCards && Card < AspectCards.Count ? AspectCards[Card] : null;

        /// <summary>The shape a row would be worn in (Y cycles it before A).</summary>
        public int ShapeOf(SpellCard row) => row != null && previewShapes.TryGetValue(row.Id, out int shape) ? shape : row?.Shape ?? 0;

        /// <summary>The aspect whose look the weapon picture shows: the focused card's while choosing, else the active one.</summary>
        public string PreviewLook => FocusedCard?.Id ?? Board.Aspect.Id;

        /// <summary>The keystone cells as shown: the focused bough's while choosing an aspect, else the active bough's.</summary>
        public WayCard[] PreviewKeystones
        {
            get
            {
                var card = FocusedCard;
                return card != null && card.Keystones != null && card.Keystones.Length == 2 ? card.Keystones : Board.Keystones;
            }
        }

        /// <summary>Which half is folded: none at 100%; at 125% and 150% the half without focus, or the one a list covers.</summary>
        public LoadoutHalf Folded
        {
            get
            {
                if (TextSize <= TextSizes.Normal) return LoadoutHalf.None;
                if (Mode == LoadoutMode.SpellList) return LoadoutHalf.Steel;
                if (Mode == LoadoutMode.AspectCards) return LoadoutHalf.Element;
                return LoadoutPlaces.InSteel(Focus) ? LoadoutHalf.Element : LoadoutHalf.Steel;
            }
        }

        // ---- Opening and reading.

        /// <summary>Opens on the last place changed at any fire (West the first time), the build read afresh.</summary>
        public void Open()
        {
            IsOpen = true;
            Mode = LoadoutMode.Board;
            ListSlot = -1;
            grid.Forget();
            previewShapes.Clear();
            Notice = "";
            CancelKeep();
            Board = source.Read() ?? new LoadoutBoard();
            Focus = lastChanged;
            if (LoadoutPlaces.InSteel(Focus)) steelLast = Focus;
            else elementLast = Focus;
            Refresh();
            Sound(MenuSound.Open);
        }

        /// <summary>Reads the build again (after a change made here, or one made elsewhere while open).</summary>
        public void Reread()
        {
            Board = source.Read() ?? new LoadoutBoard();
            Refresh();
        }

        public void Close()
        {
            IsOpen = false;
            Mode = LoadoutMode.Board;
            CancelKeep();
        }

        // ---- The stick.

        /// <summary>A push of the stick, the d-pad or an arrow key.</summary>
        public void Move(NavDirection direction)
        {
            if (!IsOpen) return;
            CancelKeep();
            switch (Mode)
            {
                case LoadoutMode.Board:
                {
                    int to = grid.Move((int)Focus, direction);
                    if (to == (int)Focus)
                    {
                        Sound(MenuSound.Blocked);
                        return;
                    }
                    SetFocus((LoadoutPlace)to);
                    Sound(MenuSound.Move);
                    return;
                }
                case LoadoutMode.SpellList:
                {
                    int column = Column, row = Row;
                    if (direction == NavDirection.Up) row--;
                    else if (direction == NavDirection.Down) row++;
                    else column += direction == NavDirection.Left ? -1 : 1;
                    if (column < 0 || column >= Columns.Count || row < 0 || row >= Columns[column].Rows.Count)
                    {
                        // Across to a shorter column, the row is the last that column has.
                        if (column >= 0 && column < Columns.Count && column != Column && Columns[column].Rows.Count > 0) row = Columns[column].Rows.Count - 1;
                        else
                        {
                            Sound(MenuSound.Blocked);
                            return;
                        }
                    }
                    Column = column;
                    Row = row;
                    Notice = "";
                    Refresh();
                    Sound(MenuSound.Move);
                    return;
                }
                default:
                {
                    int card = Card + (direction == NavDirection.Left ? -1 : direction == NavDirection.Right ? 1 : 0);
                    if (card == Card || card < 0 || card >= AspectCards.Count)
                    {
                        Sound(MenuSound.Blocked);
                        return;
                    }
                    Card = card;
                    Refresh();
                    Sound(MenuSound.Move);
                    return;
                }
            }
        }

        private void SetFocus(LoadoutPlace place)
        {
            Focus = place;
            if (LoadoutPlaces.InSteel(place)) steelLast = place;
            else elementLast = place;
            Notice = "";
            Refresh();
        }

        // ---- A, B, X, Y.

        /// <summary>A (use): opens a slot's list or the aspect cards, sets a keystone or bearing, wears a Hand; in a list, chooses and closes it.</summary>
        public void Submit()
        {
            if (!IsOpen) return;
            CancelKeep();
            if (Mode == LoadoutMode.SpellList)
            {
                var row = FocusedRow;
                if (row == null)
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                int slot = ListSlot;
                bool worn = source.WearSpell(slot, row.Id, ShapeOf(row));
                CloseList();
                if (worn) lastChanged = LoadoutPlaces.PlaceOfSlot(slot);
                Reread();
                Sound(worn ? MenuSound.Set : MenuSound.Blocked);
                return;
            }
            if (Mode == LoadoutMode.AspectCards)
            {
                var card = FocusedCard;
                if (card == null || !card.Bought)
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                bool changed = !card.Set && source.SetAspect(card.Id);
                CloseList();
                if (changed) lastChanged = LoadoutPlace.Aspect;
                Reread();
                Sound(changed ? MenuSound.Set : MenuSound.Close);
                return;
            }

            int at = LoadoutPlaces.SlotOf(Focus);
            if (at >= 0)
            {
                var card = at == 4 ? Board.Surge : Board.Slots[at];
                if (!card.Open)
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                OpenList(at);
                return;
            }
            int cell = LoadoutPlaces.PairCellOf(Focus);
            if (cell >= 0)
            {
                bool bearing = Focus == LoadoutPlace.Bearing1 || Focus == LoadoutPlace.Bearing2;
                var pair = bearing ? Board.Bearings : Board.Keystones;
                var mark = pair[cell];
                if (string.IsNullOrEmpty(mark.Id) || !mark.Bought || mark.Set || !source.SetPair(bearing, mark.Id))
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                lastChanged = Focus;
                Reread();
                Sound(MenuSound.Set);
                return;
            }
            int hand = LoadoutPlaces.HandOf(Focus);
            if (hand >= 0)
            {
                if ((hand > 0 && Board.Hands[hand].Empty) || !source.WearHand(hand))
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                lastChanged = Focus;
                Reread();
                Sound(MenuSound.Set);
                return;
            }
            if (Focus == LoadoutPlace.Aspect)
            {
                OpenAspects();
                return;
            }
            if (Focus == LoadoutPlace.ElementHeader)
            {
                Sound(MenuSound.Open);
                ReactionsAsked?.Invoke();
                return;
            }
            // Temper, the technique, the infused strikes and the role strip only explain themselves.
            Sound(MenuSound.Blocked);
        }

        /// <summary>B (back): closes an open list with nothing changed. True when there was nothing to close, so the screen goes back to the fire.</summary>
        public bool Cancel()
        {
            if (!IsOpen) return true;
            CancelKeep();
            Sound(MenuSound.Close);
            if (Mode == LoadoutMode.Board) return true;
            CloseList();
            Refresh();
            return false;
        }

        /// <summary>Y: on a slot, wears its spell's next shape; on a list row, previews it (A wears it). Nothing when the Shape mark is not bought.</summary>
        public void Shape()
        {
            if (!IsOpen) return;
            CancelKeep();
            if (Mode == LoadoutMode.SpellList)
            {
                var row = FocusedRow;
                if (row == null || !row.HasShapes)
                {
                    Refuse(row != null && !row.IsEmpty ? row : null);
                    return;
                }
                previewShapes[row.Id] = (ShapeOf(row) + 1) % row.Shapes.Length;
                Notice = "";
                Refresh();
                Sound(MenuSound.Move);
                return;
            }
            int slot = LoadoutPlaces.SlotOf(Focus);
            if (Mode != LoadoutMode.Board || slot < 0 || slot > 3)
            {
                Sound(MenuSound.Blocked);
                return;
            }
            var card = Board.Slots[slot];
            if (!card.Open || card.IsEmpty || !card.HasShapes)
            {
                Refuse(card.Open && !card.IsEmpty ? card : null);
                return;
            }
            if (!source.ChooseShape(card.ShapeKey, (card.Shape + 1) % card.Shapes.Length))
            {
                Sound(MenuSound.Blocked);
                return;
            }
            lastChanged = Focus;
            Reread();
            Sound(MenuSound.Set);
        }

        // Y where there is no other shape: the pane's last line says where to buy one.
        private void Refuse(SpellCard spell)
        {
            Notice = spell == null ? "" : spell.ShapeHint.Length > 0 ? spell.ShapeHint : "This spell has only one shape.";
            Refresh();
            Sound(MenuSound.Blocked);
        }

        /// <summary>X pressed: on an empty saved Hand, keeps the Hand worn there at once; on one that holds a Hand, starts the 0.6 s hold.</summary>
        public void KeepDown()
        {
            if (!IsOpen) return;
            int hand = LoadoutPlaces.HandOf(Focus);
            if (Mode != LoadoutMode.Board || hand < 1)
            {
                Sound(MenuSound.Blocked);
                return;
            }
            if (Board.Hands[hand].Empty)
            {
                Keep(hand);
                return;
            }
            keepHolding = true;
            keepHand = hand;
            KeepProgress = 0f;
        }

        /// <summary>X still held, <paramref name="seconds"/> later: fills the ring, and keeps the Hand once it is full.</summary>
        public void KeepHeld(float seconds)
        {
            if (!keepHolding) return;
            KeepProgress = Math.Min(1f, KeepProgress + seconds / KeepHoldSeconds);
            if (KeepProgress < 1f) return;
            keepHolding = false;
            KeepProgress = 0f;
            Keep(keepHand);
        }

        /// <summary>X let go: a hold not yet full keeps nothing.</summary>
        public void KeepUp() => CancelKeep();

        private void CancelKeep()
        {
            keepHolding = false;
            KeepProgress = 0f;
        }

        private void Keep(int hand)
        {
            if (!source.KeepHand(hand))
            {
                Sound(MenuSound.Blocked);
                return;
            }
            lastChanged = LoadoutPlace.Starter + hand;
            Reread();
            Sound(MenuSound.Set);
        }

        // ---- LB, RB, L3, the right stick.

        /// <summary>LB (Steel) or RB (Element): jumps to that half where focus last was; in a spell list, to the element group before or after.</summary>
        public void JumpHalf(bool steel)
        {
            if (!IsOpen) return;
            CancelKeep();
            if (Mode == LoadoutMode.SpellList)
            {
                int column = Column + (steel ? -1 : 1);
                if (column < 0 || column >= Columns.Count)
                {
                    Sound(MenuSound.Blocked);
                    return;
                }
                Column = column;
                Row = Math.Min(Row, Math.Max(0, Columns[column].Rows.Count - 1));
                Refresh();
                Sound(MenuSound.Move);
                return;
            }
            var target = steel ? steelLast : elementLast;
            if (Mode != LoadoutMode.Board || LoadoutPlaces.InSteel(Focus) == steel)
            {
                Sound(MenuSound.Blocked);
                return;
            }
            SetFocus(target);
            Sound(MenuSound.Move);
        }

        /// <summary>L3 (T): steps the text size 100, 125, 150, then back to 100.</summary>
        public void StepTextSize()
        {
            TextSize = TextSizes.Next(TextSize);
            TextSizeChanged?.Invoke(TextSize);
            Refresh();
            Sound(MenuSound.Move);
        }

        /// <summary>Sets the text size without stepping (the pause menu's setting changed it).</summary>
        public void SetTextSize(int size)
        {
            int normal = TextSizes.Normalise(size);
            if (normal == TextSize) return;
            TextSize = normal;
            Refresh();
        }

        /// <summary>The right stick or the page keys: scrolls the pane.</summary>
        public void ScrollPane(float lines)
        {
            if (lines != 0f) PaneScrolled?.Invoke(lines);
        }

        // ---- The mouse.

        /// <summary>The pointer is over a place: it takes focus, so the pane follows the pointer. Ignored while a list is open.</summary>
        public void PointAt(LoadoutPlace place)
        {
            if (!IsOpen || Mode != LoadoutMode.Board || place == Focus) return;
            CancelKeep();
            SetFocus(place);
            Sound(MenuSound.Move);
        }

        /// <summary>A left click on a place: focuses it and presses A on it.</summary>
        public void Click(LoadoutPlace place)
        {
            if (!IsOpen || Mode != LoadoutMode.Board) return;
            PointAt(place);
            Submit();
        }

        /// <summary>The pointer over a spell list's row.</summary>
        public void PointAtRow(int column, int row)
        {
            if (Mode != LoadoutMode.SpellList || column < 0 || column >= Columns.Count || row < 0 || row >= Columns[column].Rows.Count) return;
            if (column == Column && row == Row) return;
            Column = column;
            Row = row;
            Notice = "";
            Refresh();
            Sound(MenuSound.Move);
        }

        public void ClickRow(int column, int row)
        {
            PointAtRow(column, row);
            if (Mode == LoadoutMode.SpellList && column == Column && row == Row) Submit();
        }

        /// <summary>The pointer over an aspect card.</summary>
        public void PointAtCard(int card)
        {
            if (Mode != LoadoutMode.AspectCards || card < 0 || card >= AspectCards.Count || card == Card) return;
            Card = card;
            Refresh();
            Sound(MenuSound.Move);
        }

        public void ClickCard(int card)
        {
            PointAtCard(card);
            if (Mode == LoadoutMode.AspectCards && card == Card) Submit();
        }

        // ---- Lists.

        private void OpenList(int slot)
        {
            choices.Clear();
            source.SpellChoices(slot, choices);
            Columns.Clear();
            previewShapes.Clear();
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                string group = choice.IsDuo ? "Both" : choice.Element ?? "";
                SpellColumn column = null;
                foreach (var each in Columns)
                    if (each.Element == group) column = each;
                if (column == null)
                {
                    column = new SpellColumn { Element = group };
                    Columns.Add(column);
                }
                column.Rows.Add(choice);
            }
            // The duo spells' column goes last, after the elements'.
            for (int i = 0; i < Columns.Count - 1; i++)
                if (Columns[i].Element == "Both")
                {
                    var both = Columns[i];
                    Columns.RemoveAt(i);
                    Columns.Add(both);
                    break;
                }
            if (slot < 4)
            {
                if (Columns.Count == 0) Columns.Add(new SpellColumn { Element = Board.First });
                Columns[0].Rows.Add(new SpellCard { Id = "", Name = "Leave empty", Line = "Nothing goes in this slot." });
            }
            if (Columns.Count == 0)
            {
                Sound(MenuSound.Blocked);
                return;
            }
            // Focus opens on the spell worn here, or the first row.
            string worn = slot == 4 ? Board.Surge.Id : Board.Slots[slot].Id;
            Column = 0;
            Row = 0;
            for (int c = 0; c < Columns.Count; c++)
                for (int r = 0; r < Columns[c].Rows.Count; r++)
                    if (Columns[c].Rows[r].Id == worn && (worn.Length > 0 || slot < 4))
                    {
                        Column = c;
                        Row = r;
                    }
            ListSlot = slot;
            Mode = LoadoutMode.SpellList;
            Notice = "";
            Refresh();
            Sound(MenuSound.Open);
        }

        private void OpenAspects()
        {
            AspectCards.Clear();
            source.AspectChoices(AspectCards);
            if (AspectCards.Count == 0)
            {
                Sound(MenuSound.Blocked);
                return;
            }
            Card = 0;
            for (int i = 0; i < AspectCards.Count; i++)
                if (AspectCards[i].Set) Card = i;
            Mode = LoadoutMode.AspectCards;
            Notice = "";
            Refresh();
            Sound(MenuSound.Open);
        }

        private void CloseList()
        {
            if (Mode == LoadoutMode.SpellList && ListSlot >= 0) Focus = LoadoutPlaces.PlaceOfSlot(ListSlot);
            else if (Mode == LoadoutMode.AspectCards) Focus = LoadoutPlace.Aspect;
            Mode = LoadoutMode.Board;
            ListSlot = -1;
            previewShapes.Clear();
            Notice = "";
        }

        // ---- Showing.

        private void Refresh()
        {
            Count(Board.Slots, Roles);
            Preview();
            MissingRoles = LoadoutWords.Missing(RolesPreview);
            Describe();
            Version++;
            Changed?.Invoke();
        }

        private void Sound(MenuSound sound) => Sounded?.Invoke(sound);

        // The role counts the focused choice would give: a list row in the slot (a spell from another slot changes
        // places), or a saved Hand worn.
        private void Preview()
        {
            Array.Copy(Roles, RolesPreview, Roles.Length);
            var row = FocusedRow;
            if (row != null && ListSlot >= 0 && ListSlot < 4)
            {
                Array.Clear(RolesPreview, 0, RolesPreview.Length);
                for (int i = 0; i < Board.Slots.Length; i++)
                {
                    var spell = i == ListSlot ? row : i == row.InSlot ? Board.Slots[ListSlot] : Board.Slots[i];
                    if (Board.Slots[i].Open && !spell.IsEmpty) RolesPreview[(int)spell.Role]++;
                }
                return;
            }
            int hand = Mode == LoadoutMode.Board ? LoadoutPlaces.HandOf(Focus) : -1;
            if (hand >= 0 && !Board.Hands[hand].Empty) Count(Board.Hands[hand].Slots, RolesPreview);
        }

        private static void Count(SpellCard[] slots, int[] into)
        {
            Array.Clear(into, 0, into.Length);
            foreach (var spell in slots)
                if (spell != null && spell.Open && !spell.IsEmpty) into[(int)spell.Role]++;
        }

        /// <summary>The role strip in letters: "O1 B1 D1 C1 M0", from the preview.</summary>
        public string RoleLetters()
        {
            var words = new System.Text.StringBuilder(24);
            foreach (var role in LoadoutWords.Roles)
            {
                if (words.Length > 0) words.Append(' ');
                words.Append(LoadoutWords.RoleLetter(role)).Append(RolesPreview[(int)role]);
            }
            return words.ToString();
        }

        /// <summary>Steel folded to one line (3.5.8): "Katana · The Sheath · The Cut Lands at the Click · The Light Body · Temper II".</summary>
        public string SteelSummary()
        {
            var words = new System.Text.StringBuilder(96);
            words.Append(Board.Weapon.Length > 0 ? Board.Weapon : "No weapon");
            if (!string.IsNullOrEmpty(Board.Aspect.Id)) words.Append(" · ").Append(Board.Aspect.Name);
            foreach (var keystone in Board.Keystones)
                if (keystone.Set) words.Append(" · ").Append(keystone.Name);
            foreach (var bearing in Board.Bearings)
                if (bearing.Set) words.Append(" · ").Append(bearing.Name);
            words.Append(" · ").Append(LoadoutWords.TemperName(Board.Temper));
            return words.ToString();
        }

        /// <summary>Element folded to one line's words (the view adds the spells' icons between): "Fire + Water" and the role letters.</summary>
        public string ElementSummary() => Board.ElementWords + " · " + RoleLetters();

        /// <summary>The spell list's top line: "Choosing for {SpellWest} West", with the slot's press as a glyph.</summary>
        public string ListTitle => ListSlot < 0 ? "" : ListSlot == 4 ? "Choosing the surge" : "Choosing for {" + LoadoutWords.SlotActions[ListSlot] + "} " + LoadoutWords.SlotWords[ListSlot];
    }
}
