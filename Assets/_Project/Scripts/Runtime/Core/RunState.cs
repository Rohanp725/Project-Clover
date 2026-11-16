using System.Collections.Generic;
using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// What the player carries from one scene to the next within a run: the weapon in hand, the element, what was
    /// chosen at the last god, the upgrades learned and the points to learn more. It is plain remembered state; a new
    /// play session, and a new run, start it clean.
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
        /// <summary>Embers taken from the fallen and not yet spent.</summary>
        public static int Points;
        /// <summary>The ids of the upgrades learned in this run.</summary>
        public static readonly HashSet<string> Unlocked = new HashSet<string>();
        /// <summary>The names of every element that has been in the player's hands in this run, the one held now among them.</summary>
        public static readonly HashSet<string> ElementsHeld = new HashSet<string>();
        /// <summary>The names of the elements whose gods were consumed in this run. With those held, they decide which mixed branches are open.</summary>
        public static readonly HashSet<string> ElementsConsumed = new HashSet<string>();
        /// <summary>The id of the learned spell worn in the hold-spell slot, or empty for none.</summary>
        public static string WornSpell = "";
        /// <summary>Counts some marks keep for the whole run (folds of a blade), by the mark's id.</summary>
        public static readonly Dictionary<string, int> Tallies = new Dictionary<string, int>();
        /// <summary>How many runs have ended in this play session.</summary>
        public static int RunsEnded;
        /// <summary>How the last run ended: true if the player fell, false if they came back by their own road.</summary>
        public static bool LastRunFell;
        /// <summary>How many gods fell in the last run.</summary>
        public static int LastRunGods;

        // Domain reload is off: a new play must not inherit the last one's run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            Reset();
            RunsEnded = 0;
            LastRunFell = false;
            LastRunGods = 0;
        }

        /// <summary>Ends the run: what happened is remembered for the hub, and everything the run carried is given up.</summary>
        public static void End(bool fell)
        {
            LastRunFell = fell;
            LastRunGods = GodsFelled;
            RunsEnded++;
            Reset();
        }

        /// <summary>Starts a new run: everything a run carries is put back to nothing.</summary>
        public static void Reset()
        {
            Weapon = "";
            Element = "";
            ElementConsumed = false;
            GodsFelled = 0;
            Points = 0;
            Unlocked.Clear();
            ElementsHeld.Clear();
            ElementsConsumed.Clear();
            WornSpell = "";
            Tallies.Clear();
        }
    }
}
