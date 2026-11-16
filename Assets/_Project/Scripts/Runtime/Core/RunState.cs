using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// What the player carries from one scene to the next within a run: the weapon in hand, the element, and what was
    /// chosen at the last god. It is plain remembered state; a new play session starts it clean.
    /// </summary>
    public static class RunState
    {
        /// <summary>The weapon's name, or empty to use whatever the scene gives the player.</summary>
        public static string Weapon = "";
        /// <summary>The element's name, or empty for the scene's own.</summary>
        public static string Element = "";
        /// <summary>True once the element has been strengthened by consuming a god.</summary>
        public static bool ElementConsumed;
        /// <summary>How many gods have fallen in this run.</summary>
        public static int GodsFelled;

        // Domain reload is off: a new play must not inherit the last one's run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Weapon = "";
            Element = "";
            ElementConsumed = false;
            GodsFelled = 0;
        }
    }
}
