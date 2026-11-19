using System;

namespace Clover.Player.Build
{
    /// <summary>
    /// What of the Hand is open (STEEL_AND_ELEMENT.md 4.3, IMPLEMENTATION.md 3.6): at the start one element, two spell
    /// slots (West and North) and the surge; the third slot (East) once the Champion of the Gate falls; the fourth
    /// (South) at the Fire god; the second attunement after any god. In the playtest everything is open. Plain C#: the
    /// flags are asked of whatever keeps them (<c>Progress.Has</c> in play, a set in tests).
    /// </summary>
    public static class SlotSchedule
    {
        /// <summary>The progress flag the Champion of the Gate sets when beaten (wired by task 2.4).</summary>
        public const string ChampionFlag = "elite.champion_gate";
        /// <summary>The progress flag the Fire god sets when felled (wired by task 2.4).</summary>
        public const string FireGodFlag = "god.fire";
        public const int StartingSlots = 2;

        /// <summary>
        /// How much of the Hand is open. <paramref name="has"/> answers whether a progress flag is set; a null one sets
        /// none. <paramref name="godsFelled"/> counts every god felled, whichever.
        /// </summary>
        public static HandOpen Open(Func<string, bool> has, int godsFelled, bool playtest)
        {
            if (playtest) return HandOpen.All;
            int slots = StartingSlots;
            if (has != null && has(ChampionFlag)) slots = 3;
            if (has != null && has(FireGodFlag)) slots = 4;
            bool secondAttunement = godsFelled > 0 || (has != null && has(FireGodFlag));
            return new HandOpen(slots, true, secondAttunement);
        }
    }
}
