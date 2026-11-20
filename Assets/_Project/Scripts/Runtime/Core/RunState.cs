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
        public static string Weapon = ""; // moves in Phase 5: SaveData.Current.Weapon
        /// <summary>The element's name, or empty for the scene's own.</summary>
        public static string Element = ""; // moves in Phase 5: SaveData.Current.Attunements[0]
        /// <summary>The second attunement's name (the Hand's second element), or empty for one element carried.</summary>
        public static string Element2 = ""; // moves in Phase 5: SaveData.Current.Attunements[1]
        /// <summary>The surge the Hand wears (<c>fire.surge</c>), or empty for the first element's.</summary>
        public static string Surge = ""; // moves in Phase 5: SaveData.Current.Surge
        /// <summary>True once a Hand has been set at a rest fire: the slots then stop filling themselves.</summary>
        public static bool HandSet; // moves in Phase 5: with the Hand into SaveData.Current
        /// <summary>The three saved Hands (only their Hand fields are ever swapped in); null for an empty one.</summary>
        public static readonly Clover.Player.Build.Loadout[] SavedHands = new Clover.Player.Build.Loadout[3]; // moves in Phase 5: SaveData.SavedHands
        /// <summary>True once the element has been strengthened by consuming a god.</summary>
        public static bool ElementConsumed; // moves in Phase 5: retired, not saved (IMPLEMENTATION.md 5.1)
        /// <summary>How many gods have fallen in this run.</summary>
        public static int GodsFelled; // moves in Phase 5: SaveData.GodsFelled
        /// <summary>Embers taken from the fallen and not yet spent.</summary>
        public static int Points; // moves in Phase 5: SaveData.Wallet.Shards (task 5.1)
        /// <summary>The ids of the upgrades learned in this run.</summary>
        public static readonly HashSet<string> Unlocked = new HashSet<string>(); // moves in Phase 5: SaveData.Learned
        /// <summary>The names of every element that has been in the player's hands in this run, the one held now among them.</summary>
        public static readonly HashSet<string> ElementsHeld = new HashSet<string>(); // moves in Phase 5: retired, not saved (IMPLEMENTATION.md 5.1)
        /// <summary>The names of the elements whose gods were consumed in this run. With those held, they decide which mixed branches are open.</summary>
        public static readonly HashSet<string> ElementsConsumed = new HashSet<string>(); // moves in Phase 5: the plan names no field; settled with Take and Consume (task L.2)
        /// <summary>What sits in the four spell slots (West, North, East, South), by mark id or base id; empty for none.</summary>
        public static readonly string[] SpellSlots = { "", "", "", "" }; // moves in Phase 5: SaveData.Current.Slots
        /// <summary>Which shape each spell wears (by its branch's Learn id), chosen at a rest fire.</summary>
        public static readonly List<Clover.Player.Build.IdChoice> Shapes = new List<Clover.Player.Build.IdChoice>(); // moves in Phase 5: SaveData.Current.Shapes
        /// <summary>Counts some marks keep for the whole run (folds of a blade), by the mark's id.</summary>
        public static readonly Dictionary<string, int> Tallies = new Dictionary<string, int>(); // moves in Phase 5: SaveData.Tallies
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
            Element2 = "";
            Surge = "";
            HandSet = false;
            for (int i = 0; i < SavedHands.Length; i++) SavedHands[i] = null;
            ElementConsumed = false;
            GodsFelled = 0;
            Points = 0;
            Unlocked.Clear();
            ElementsHeld.Clear();
            ElementsConsumed.Clear();
            for (int i = 0; i < SpellSlots.Length; i++) SpellSlots[i] = "";
            Shapes.Clear();
            Tallies.Clear();
        }
    }
}
