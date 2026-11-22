using System;
using System.Collections.Generic;
using Clover.Player;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// A spell as the Loadout shows it: in a slot, as the surge, in a spell list's row, or in a saved Hand. Plain words
    /// and numbers read from the build once, so the screen and the tests need no player.
    /// </summary>
    public sealed class SpellCard
    {
        /// <summary>The mark id or base id (<c>fire.base</c>), the surge id, or "" for an empty slot or "Leave empty".</summary>
        public string Id = "";
        public string Name = "";
        /// <summary>The element; a duo's first, with the other in <see cref="Element2"/>.</summary>
        public string Element = "";
        public string Element2 = "";
        public SpellRole Role;
        /// <summary>Paid in mana (its price); 0 for a spell that waits on a cooldown.</summary>
        public float Mana;
        public float Cooldown;
        /// <summary>What it does, one sentence of 15 words or fewer.</summary>
        public string Line = "";
        /// <summary>The long words, for the pane under the rule.</summary>
        public string Details = "";
        /// <summary>What the shape choice is kept under (the spell branch's Learn id); "" when it has none.</summary>
        public string ShapeKey = "";
        /// <summary>The names of its shapes when its Shape mark is bought; empty otherwise.</summary>
        public string[] Shapes = Array.Empty<string>();
        /// <summary>The shape worn (an index into <see cref="Shapes"/>).</summary>
        public int Shape;
        /// <summary>Where to buy its Shape mark when it is not bought ("Buy Flame Wave's Shape on the element chart. 3 Shards."); "" when it has none.</summary>
        public string ShapeHint = "";
        /// <summary>False for a slot the schedule keeps shut.</summary>
        public bool Open = true;
        /// <summary>A shut slot's short reason ("Opens at the Champion") and its whole sentence for the pane.</summary>
        public string ShutReason = "";
        public string ShutDetail = "";
        /// <summary>In a spell list: the slot it sits in now (0 West to 3 South), or -1.</summary>
        public int InSlot = -1;
        /// <summary>True for the surge (it has no role and no shape).</summary>
        public bool IsSurge;

        public bool IsEmpty => string.IsNullOrEmpty(Id);
        public bool IsDuo => !string.IsNullOrEmpty(Element2);
        public bool HasShapes => Shapes != null && Shapes.Length > 1;

        /// <summary>The name of the shape worn, or "".</summary>
        public string ShapeName => Shapes != null && Shapes.Length > 0 ? Shapes[Math.Clamp(Shape, 0, Shapes.Length - 1)] : "";

        /// <summary>The price as the slot shows it: "20 mana" or "7 s"; "" for nothing.</summary>
        public string CostShort => Mana > 0f ? Round(Mana) + " mana" : Cooldown > 0f ? Round(Cooldown) + " s" : "";

        /// <summary>The cost line of the pane: "20 mana", "Ready again in 7 s", or "Costs nothing".</summary>
        public string CostLine => Mana > 0f ? Round(Mana) + " mana" : Cooldown > 0f ? "Ready again in " + Round(Cooldown) + " s" : "Costs nothing";

        public static int Round(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        /// <summary>A copy with its own shapes array, so a preview never changes the board.</summary>
        public SpellCard Clone()
        {
            var copy = (SpellCard)MemberwiseClone();
            copy.Shapes = Shapes != null ? (string[])Shapes.Clone() : Array.Empty<string>();
            return copy;
        }
    }

    /// <summary>One of the Way's choices as the Loadout shows it: an aspect, a keystone, a bearing, Temper or the technique.</summary>
    public sealed class WayCard
    {
        public string Id = "";
        public string Name = "";
        /// <summary>The painted icon's key (<see cref="IconShapes"/>).</summary>
        public string Icon = "";
        /// <summary>The kind line: "Keystone of the Sheath", "Bearing of the Body", "Aspect of the Katana".</summary>
        public string Kind = "";
        /// <summary>A few words for a card's second line ("Hits push you less").</summary>
        public string Short = "";
        /// <summary>What it does, in full.</summary>
        public string Words = "";
        /// <summary>What turning it off takes away, for "You lose: ..." ("the long held draw that cuts through guards").</summary>
        public string Gives = "";
        /// <summary>Its cost when used ("10 stamina a draw"), or "".</summary>
        public string Cost = "";
        /// <summary>Its press, with action names in braces ("Hold {LightAttack}, then let go"), or "".</summary>
        public string Press = "";
        public bool Bought;
        /// <summary>The set one of a pair, or the active aspect.</summary>
        public bool Set;
        /// <summary>Its price in Memories.</summary>
        public int Price;
        /// <summary>Why it cannot be had now, and how ("Buy it on the Way chart. 4 Memories."); "" when it can.</summary>
        public string Lock = "";
        /// <summary>An aspect's marks bought and in all ("6 of 10 marks bought").</summary>
        public int MarksBought;
        public int MarksTotal;
        /// <summary>An aspect's two keystones, for the preview while choosing an aspect.</summary>
        public WayCard[] Keystones = Array.Empty<WayCard>();
    }

    /// <summary>The Starter or a saved Hand, as the Hands row shows it.</summary>
    public sealed class HandCard
    {
        public string Name = "";
        /// <summary>True for a saved Hand with nothing kept.</summary>
        public bool Empty = true;
        /// <summary>True when it matches the Hand worn now.</summary>
        public bool Worn;
        /// <summary>Its four spells (West, North, East, South) and its surge.</summary>
        public SpellCard[] Slots = { new SpellCard(), new SpellCard(), new SpellCard(), new SpellCard() };
        public SpellCard Surge = new SpellCard { IsSurge = true };
    }

    /// <summary>Everything the Loadout shows, read from the build in one go (ui_rest_fire_plan.md 3.0 and 3.5).</summary>
    public sealed class LoadoutBoard
    {
        public const int TemperMost = 5;

        public int Memories;
        public int Shards;
        /// <summary>The carried weapon's kind ("Katana"), whose Way the Steel half sets (Q5: the carried weapon only).</summary>
        public string Weapon = "";
        /// <summary>The active aspect (or a card saying there is none).</summary>
        public WayCard Aspect = new WayCard();
        /// <summary>The active bough's two keystones, top first; a card with no id where there is none.</summary>
        public WayCard[] Keystones = { new WayCard(), new WayCard() };
        /// <summary>The Body's two bearings.</summary>
        public WayCard[] Bearings = { new WayCard(), new WayCard() };
        public int Temper;
        /// <summary>The next Temper rank's price, or 0 once all five are bought.</summary>
        public int TemperNextPrice;
        public WayCard Technique = new WayCard();
        /// <summary>The elements carried, first and second ("" for none). The same one twice is all in.</summary>
        public string[] Elements = { "", "" };
        /// <summary>The Devotion's name while all in, else "".</summary>
        public string Devotion = "";
        /// <summary>The infused strikes: the element each lays; the second may be the all-in deep strike.</summary>
        public string StrikeFirst = "";
        public string StrikeSecond = "";
        public bool StrikeSecondDeep;
        public SpellCard[] Slots = { new SpellCard(), new SpellCard(), new SpellCard(), new SpellCard() };
        public SpellCard Surge = new SpellCard { IsSurge = true };
        /// <summary>The Starter (0) and the three saved Hands (1 to 3).</summary>
        public HandCard[] Hands = { new HandCard { Name = "Starter" }, new HandCard { Name = "Hand 1" }, new HandCard { Name = "Hand 2" }, new HandCard { Name = "Hand 3" } };

        public string First => Elements != null && Elements.Length > 0 ? Elements[0] ?? "" : "";
        public string Second => Elements != null && Elements.Length > 1 ? Elements[1] ?? "" : "";
        public bool AllIn => First.Length > 0 && First == Second;

        /// <summary>"Fire + Water", "Fire, all in", or "No element".</summary>
        public string ElementWords => First.Length == 0 ? "No element" : AllIn ? First + ", all in" : Second.Length == 0 ? First : First + " + " + Second;
    }

    /// <summary>One column of a spell list: an element's spells under its coloured header, or "Both" for duo spells.</summary>
    public sealed class SpellColumn
    {
        /// <summary>The element ("Fire"), or "Both" for the duo spells; the view colours the header by it.</summary>
        public string Element = "";
        public readonly List<SpellCard> Rows = new List<SpellCard>();
    }

    /// <summary>
    /// Where the Loadout reads the build and makes its choices. <see cref="GameLoadoutSource"/> binds it to the player's
    /// build, the run's Way choices and the wallet; tests give it a stand-in. Every change is made only at a rest fire, so
    /// the source is only ever made there.
    /// </summary>
    public interface ILoadoutSource
    {
        /// <summary>The whole build as it stands now. May return the same object each time, refilled.</summary>
        LoadoutBoard Read();

        /// <summary>What can go in a slot (0 West to 3 South), or the surges for slot 4, in the chart's order.</summary>
        void SpellChoices(int slot, List<SpellCard> into);

        /// <summary>The carried weapon's aspects: the three plain boughs, then the god's once taken; bought or not.</summary>
        void AspectChoices(List<WayCard> into);

        /// <summary>Wears a spell (or "" for nothing) in a slot, or a surge for slot 4, in the shape given. A spell worn in another slot changes places with what was here.</summary>
        bool WearSpell(int slot, string id, int shape);

        /// <summary>Wears a spell's other shape, by its shape key.</summary>
        bool ChooseShape(string shapeKey, int shape);

        /// <summary>Makes an aspect the carried weapon's active one (the weapon is re-armed).</summary>
        bool SetAspect(string aspectId);

        /// <summary>Sets a keystone of the active bough, or a bearing of the Body.</summary>
        bool SetPair(bool bearing, string markId);

        /// <summary>Wears the Starter (0) or a saved Hand (1 to 3).</summary>
        bool WearHand(int hand);

        /// <summary>Keeps the Hand worn now as saved Hand 1 to 3.</summary>
        bool KeepHand(int hand);
    }

    /// <summary>Where a line of the pane sits: the words, the facts (cost, press, shape), the compare block, or the foot.</summary>
    public enum PaneSection
    {
        Body,
        Facts,
        Compare,
        Foot,
    }

    /// <summary>How a line of the pane is coloured.</summary>
    public enum PaneTone
    {
        Plain,
        Meta,
        Memories,
        Loss,
        Keyword,
    }

    /// <summary>One line of the pane. Its text may name presses in braces ("Press {LightAttack}"), drawn as glyphs.</summary>
    public struct PaneLine
    {
        public PaneSection Section;
        public PaneTone Tone;
        /// <summary>Bold words in front in the line's tone ("You lose:", "Draw:"); "" for none.</summary>
        public string Lead;
        public string Text;

        public PaneLine(PaneSection section, string text, PaneTone tone = PaneTone.Plain, string lead = "")
        {
            Section = section;
            Text = text ?? "";
            Tone = tone;
            Lead = lead ?? "";
        }
    }

    /// <summary>
    /// The details pane (ui_rest_fire_plan.md 4.4 and 3.5.6): always on, always describing the focused thing. The name,
    /// the kind line, an optional price, then lines by section, then the one press that acts here.
    /// </summary>
    public sealed class LoadoutPane
    {
        public string Icon = "";
        /// <summary>The element whose colour tints the icon, or "".</summary>
        public string IconElement = "";
        public string Title = "";
        public string Kind = "";
        /// <summary>"Bought for 4 Memories", in the Memories colour; "" for none.</summary>
        public string Price = "";
        public readonly List<PaneLine> Lines = new List<PaneLine>();
        /// <summary>The UI action whose glyph leads the last line ("Submit", "Keep"), or "" when nothing acts here.</summary>
        public string Action = "";
        public string ActionText = "";
        /// <summary>True when the action is a hold (a kept Hand replaced).</summary>
        public bool ActionHold;

        public void Clear()
        {
            Icon = IconElement = Title = Kind = Price = Action = ActionText = "";
            ActionHold = false;
            Lines.Clear();
        }

        public void Add(PaneSection section, string text, PaneTone tone = PaneTone.Plain, string lead = "")
        {
            if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(lead)) Lines.Add(new PaneLine(section, text, tone, lead));
        }

        /// <summary>Every word of the pane in one string, for tests and probes.</summary>
        public string AllText()
        {
            var words = new System.Text.StringBuilder(256);
            words.Append(Title).Append('\n').Append(Kind).Append('\n').Append(Price).Append('\n');
            foreach (var line in Lines) words.Append(line.Lead).Append(line.Lead.Length > 0 ? " " : "").Append(line.Text).Append('\n');
            words.Append(ActionText);
            return words.ToString();
        }
    }
}
