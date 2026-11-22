using System.Collections.Generic;

namespace Clover.UI
{
    /// <summary>
    /// What a rest fire offers and which entry is chosen (IMPLEMENTATION.md 4.1): the Loadout (the Hand, the Way's
    /// choices and the spells' shapes; the reactions' shapes on its Reactions page), the chart, the Way (once it is built, phase 5), the road to the hub (once it is
    /// built, Track L), and leave. Plain state with no scene
    /// behind it, like <see cref="PauseChoices"/>, so the rules can be tested.
    /// </summary>
    public class RestChoices
    {
        private readonly List<RestEntry> entries = new List<RestEntry>();

        public int Count => entries.Count;
        public int Selected { get; private set; }
        public RestEntry Current => entries[Selected];

        /// <summary>The entries built so far: the Loadout, the chart and leave.</summary>
        public RestChoices() : this(false, false) { }

        /// <summary>The entries, with the Way and the road to the hub once each exists.</summary>
        public RestChoices(bool way, bool hub)
        {
            entries.Add(RestEntry.Hand);
            entries.Add(RestEntry.Chart);
            if (way) entries.Add(RestEntry.Way);
            if (hub) entries.Add(RestEntry.Hub);
            entries.Add(RestEntry.Leave);
        }

        public RestEntry At(int index) => entries[index];

        public int IndexOf(RestEntry entry) => entries.IndexOf(entry);

        /// <summary>Puts the menu back as it is when first opened: on the Hand.</summary>
        public void Reset() => Selected = 0;

        /// <summary>Moves the choice up or down, round the ends.</summary>
        public void Move(int by) => Selected = ((Selected + by) % Count + Count) % Count;

        /// <summary>Chooses an entry by its place (a click on its row). False when there is none there.</summary>
        public bool Choose(int index)
        {
            if (index < 0 || index >= Count) return false;
            Selected = index;
            return true;
        }

        /// <summary>The use button on the chosen entry: nothing here asks twice, since nothing at a fire cannot be undone.</summary>
        public RestEntry Press() => Current;

        public static string Label(RestEntry entry)
        {
            switch (entry)
            {
                case RestEntry.Hand: return "Loadout";
                case RestEntry.Chart: return "The chart";
                case RestEntry.Way: return "The Way";
                case RestEntry.Hub: return "To the hub";
                case RestEntry.Leave: return "Leave the fire";
                default: return "";
            }
        }

        /// <summary>What the entry does, shown beside the list.</summary>
        public static string Note(RestEntry entry)
        {
            switch (entry)
            {
                case RestEntry.Hand: return "Your spells, their shapes, your saved Hands and the Way's choices for your weapon. All of it can be changed at any fire.";
                case RestEntry.Chart: return "The element trees. Shards are spent here.";
                case RestEntry.Way: return "The weapon's tree. Memories are spent here.";
                case RestEntry.Hub: return "Step out of time to the hub, to carry other elements or another weapon, and come back to this fire.";
                case RestEntry.Leave: return "Back to the road.";
                default: return "";
            }
        }
    }
}
