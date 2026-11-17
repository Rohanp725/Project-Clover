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

        public PauseChoices(string sceneName)
        {
            entries.Add(PauseEntry.Resume);
            entries.Add(PauseEntry.Controls);
            entries.Add(PauseEntry.Fullscreen);
            if (IsRunScene(sceneName)) entries.Add(PauseEntry.GiveUp);
            if (sceneName == HubScene) entries.Add(PauseEntry.Forget);
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
                case PauseEntry.Quit: return "A run is not kept.";
                default: return "";
            }
        }
    }
}
