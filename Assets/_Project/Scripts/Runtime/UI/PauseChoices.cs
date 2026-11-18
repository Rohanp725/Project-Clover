using System.Collections.Generic;

namespace Clover.UI
{
    /// <summary>
    /// What the pause menu offers in a given scene, which entry is chosen, and whether it is waiting to be told twice.
    /// The things that cannot be taken back (giving up a run, forgetting everything, quitting) are asked once more
    /// before they happen. Plain state with no scene behind it, so the rules can be tested.
    /// </summary>
    public class PauseChoices
    {
        public const string HubScene = "Hub";

        // The scenes that are part of a run: only there is there a run to give up.
        private static readonly string[] RunScenes = { "Castle", "Summit" };

        private readonly List<PauseEntry> entries = new List<PauseEntry>();

        public int Count => entries.Count;
        public int Selected { get; private set; }
        /// <summary>True while the chosen entry has been pressed once and waits for the second press.</summary>
        public bool Asking { get; private set; }
        public PauseEntry Current => entries[Selected];

        public PauseChoices(string sceneName) : this(sceneName, Clover.Core.PlaytestMode.On) { }

        /// <summary>
        /// The entries for a scene. In the playtest the guide to effects and what is new come after the controls, and
        /// in every scene but the Proving Ground itself the way back to it takes the place of giving up the run (in the
        /// playtest nothing is given up).
        /// </summary>
        public PauseChoices(string sceneName, bool playtest)
        {
            entries.Add(PauseEntry.Resume);
            entries.Add(PauseEntry.Controls);
            if (playtest)
            {
                entries.Add(PauseEntry.Guide);
                entries.Add(PauseEntry.WhatsNew);
            }
            entries.Add(PauseEntry.Fullscreen);
            if (playtest)
            {
                if (sceneName != Clover.Core.PlaytestMode.ProvingGroundScene) entries.Add(PauseEntry.ProvingGround);
            }
            else
            {
                if (IsRunScene(sceneName)) entries.Add(PauseEntry.GiveUp);
                if (sceneName == HubScene) entries.Add(PauseEntry.Forget);
            }
            entries.Add(PauseEntry.Quit);
        }

        public PauseEntry At(int index) => entries[index];

        public static bool IsRunScene(string sceneName)
        {
            foreach (string scene in RunScenes)
                if (scene == sceneName) return true;
            return false;
        }

        /// <summary>True for the entries that cannot be taken back and so are asked twice.</summary>
        public static bool AsksFirst(PauseEntry entry) => entry == PauseEntry.GiveUp || entry == PauseEntry.Forget || entry == PauseEntry.Quit;

        /// <summary>Puts the menu back as it is when first opened.</summary>
        public void Reset()
        {
            Selected = 0;
            Asking = false;
        }

        /// <summary>Moves the choice up or down, round the ends. A question left unanswered is dropped.</summary>
        public void Move(int by)
        {
            Asking = false;
            Selected = ((Selected + by) % Count + Count) % Count;
        }

        /// <summary>Drops the question, if one was asked. Returns true when there was one.</summary>
        public bool Cancel()
        {
            bool was = Asking;
            Asking = false;
            return was;
        }

        /// <summary>The use button on the chosen entry: the entry to act on now, or None when it has only asked.</summary>
        public PauseEntry Press()
        {
            if (AsksFirst(Current) && !Asking)
            {
                Asking = true;
                return PauseEntry.None;
            }
            Asking = false;
            return Current;
        }

        public static string Label(PauseEntry entry)
        {
            switch (entry)
            {
                case PauseEntry.Resume: return "Resume";
                case PauseEntry.Controls: return "Controls";
                case PauseEntry.Fullscreen: return "Fullscreen or window";
                case PauseEntry.GiveUp: return "Give up the run";
                case PauseEntry.Forget: return "Forget everything";
                case PauseEntry.Quit: return "Quit";
                case PauseEntry.Guide: return "Guide to effects";
                case PauseEntry.WhatsNew: return "What's new";
                case PauseEntry.ProvingGround: return "Return to the Proving Ground";
                default: return "";
            }
        }

        /// <summary>What the row reads while it waits for the second press.</summary>
        public static string Question(PauseEntry entry)
        {
            switch (entry)
            {
                case PauseEntry.GiveUp: return "Give up? Press again";
                case PauseEntry.Forget: return "Forget? Press again";
                case PauseEntry.Quit: return "Quit? Press again";
                default: return Label(entry);
            }
        }

        /// <summary>What choosing the entry will cost, shown beside the list.</summary>
        public static string Note(PauseEntry entry)
        {
            switch (entry)
            {
                case PauseEntry.Controls: return "The buttons, and how a fight is won.";
                case PauseEntry.Fullscreen: return "Switches between the whole screen and a window that can be resized.\nAlt and Enter do the same.";
                case PauseEntry.GiveUp: return "The run ends here.\nAll it carried is lost.";
                case PauseEntry.Forget: return "Every weapon and element won is lost.\nThe first waking comes again.";
                case PauseEntry.Quit: return Clover.Core.PlaytestMode.On ? "Leaves the playtest." : "A run is not kept.";
                case PauseEntry.Guide: return "What every affliction, state and resource does: the numbers, what sets it off, how to use it and how it looks.";
                case PauseEntry.WhatsNew: return "This playtest's version, and what each update added.";
                case PauseEntry.ProvingGround: return "Back to the testing ground, keeping everything: the weapon, the element and every mark.";
                default: return "";
            }
        }
    }
}
