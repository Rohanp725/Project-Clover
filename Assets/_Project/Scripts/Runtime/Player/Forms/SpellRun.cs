using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// One cast of a spell form that takes time: the cast it was begun with and whatever the form needs to remember
    /// while it runs. Forms keep nothing of their own between casts, so everything about a cast in progress lives
    /// here, in a place <see cref="SpellWorks"/> keeps ready. The plain fields mean whatever the form says they mean.
    /// </summary>
    public struct SpellRun
    {
        public bool Active;
        /// <summary>Which of the works' places this is: its lists of solids, marks, points and times are kept under this number.</summary>
        public int Slot;
        /// <summary>This run's own number; the player's body is held under it.</summary>
        public int Token;
        public RunningForm Form;
        public SpellCast Cast;
        /// <summary>Seconds since it began, on the game's clock: a stop of time stops it too.</summary>
        public float Age;
        public bool HoldsBody;

        public int Phase, Count, Done;
        public float T0, T1, T2, T3;
        public Vector3 A, B, C;
        public bool Flag, Flag2;
        /// <summary>The enemy this run is about, as a thing that can be struck and as a place.</summary>
        public Component Victim;
    }
}
