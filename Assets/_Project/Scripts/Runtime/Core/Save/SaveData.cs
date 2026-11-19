using System;
using System.Collections.Generic;
using Clover.Player.Build;
using Clover.Player.Growth;

namespace Clover.Core.Save
{
    /// <summary>
    /// Everything the game keeps from one sitting to the next (IMPLEMENTATION.md 5.1), in one plain object that
    /// <c>JsonUtility</c> can write: lists and pairs, never dictionaries, and no field left null. <see cref="SaveStore"/>
    /// reads and writes it; <see cref="Progress"/> keeps its flags here. Most fields wait for the task that moves them out
    /// of <see cref="RunState"/>; until then they are written empty.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>The schema this build writes. A file with a lower number is brought up by <see cref="SaveMigration.Upgrade"/>.</summary>
        public const int CurrentVersion = 1;
        /// <summary>How many Hands can be kept.</summary>
        public const int SavedHandCount = 3;

        public int Version = CurrentVersion;
        /// <summary>What <see cref="Progress.Has"/> answers: doors, ladders, walls, the sponsor met, elites and gods felled.</summary>
        public List<string> Flags = new List<string>();
        /// <summary>The elements owned for good, by name (<c>Fire</c>).</summary>
        public List<string> OwnedElements = new List<string>();
        /// <summary>The weapon kinds owned for good, by name (<c>Katana</c>).</summary>
        public List<string> OwnedWeapons = new List<string>();
        /// <summary>Mark ids bought or given.</summary>
        public List<string> Learned = new List<string>();
        /// <summary>Evolutions discovered, Body pieces picked up, Heresies picked up.</summary>
        public List<string> Found = new List<string>();
        public List<string> SeenReactions = new List<string>();
        /// <summary>The kinds of enemy defeated at least once, by <c>EnemyKind</c> id (the first defeat pays Memories).</summary>
        public List<string> KindsDefeated = new List<string>();
        public Wallet Wallet = new Wallet();
        /// <summary>The death rule's one pile; no Shards in it means there is none.</summary>
        public LostShards Lost = new LostShards();
        /// <summary>The Hand and weapon carried.</summary>
        public Loadout Current = new Loadout();
        /// <summary>The kept Hands. <c>JsonUtility</c> cannot write a null, so an unused one is a Hand with nothing in it.</summary>
        public Loadout[] SavedHands = { new Loadout(), new Loadout(), new Loadout() };
        /// <summary>Bond points by owner: <c>weapon:katana</c>, <c>element:Fire</c>, <c>pair:katana/Fire</c>.</summary>
        public List<IdPoints> Bond = new List<IdPoints>();
        /// <summary>The counts some marks keep (folds of a blade), by mark id.</summary>
        public List<IdCount> Tallies = new List<IdCount>();
        /// <summary>The rest fire the player last left for the hub.</summary>
        public RestRef LastRest = new RestRef();
        public int GodsFelled;
        /// <summary>
        /// The flags already looked up in the old PlayerPrefs store (<see cref="SaveMigration.AskPrefs"/>). A flag is asked
        /// of it once, ever, so one forgotten here never comes back from there. Not in the plan's schema: it exists because
        /// PlayerPrefs cannot list its keys, and the world's keys (doors, ladders, walls) are named in the scenes.
        /// </summary>
        public List<string> PrefsAsked = new List<string>();

        /// <summary>True when the flag is set.</summary>
        public bool HasFlag(string key) => !string.IsNullOrEmpty(key) && Flags.Contains(key);

        /// <summary>
        /// Sets a flag; a weapon's or an element's flag also owns it. False when it was already set.
        /// </summary>
        public bool SetFlag(string key)
        {
            if (string.IsNullOrEmpty(key) || Flags.Contains(key)) return false;
            Flags.Add(key);
            string owned = OwnedName(key, out var into);
            if (owned != null && !into.Contains(owned)) into.Add(owned);
            return true;
        }

        /// <summary>Clears a flag; a weapon's or an element's flag also gives up owning it. False when it was not set.</summary>
        public bool ClearFlag(string key)
        {
            if (string.IsNullOrEmpty(key) || !Flags.Remove(key)) return false;
            string owned = OwnedName(key, out var into);
            if (owned != null) into.Remove(owned);
            return true;
        }

        /// <summary>
        /// Puts back what a file written by an older build, or by hand, may lack: a missing list as an empty one, the
        /// kept Hands as three. <c>JsonUtility</c> leaves a field it does not find as the constructor made it, so this is
        /// for damaged files more than old ones.
        /// </summary>
        public void Repair()
        {
            Flags ??= new List<string>();
            OwnedElements ??= new List<string>();
            OwnedWeapons ??= new List<string>();
            Learned ??= new List<string>();
            Found ??= new List<string>();
            SeenReactions ??= new List<string>();
            KindsDefeated ??= new List<string>();
            Wallet ??= new Wallet();
            Lost ??= new LostShards();
            Current ??= new Loadout();
            Bond ??= new List<IdPoints>();
            Tallies ??= new List<IdCount>();
            LastRest ??= new RestRef();
            PrefsAsked ??= new List<string>();
            if (SavedHands == null || SavedHands.Length != SavedHandCount)
            {
                var hands = new Loadout[SavedHandCount];
                for (int i = 0; i < SavedHandCount; i++)
                    hands[i] = SavedHands != null && i < SavedHands.Length ? SavedHands[i] : null;
                SavedHands = hands;
            }
            for (int i = 0; i < SavedHands.Length; i++) SavedHands[i] ??= new Loadout();
        }

        // "weapon.Katana" owns the Katana, "element.Fire" owns Fire; any other flag owns nothing.
        private string OwnedName(string key, out List<string> into)
        {
            if (key.StartsWith(Progress.WeaponPrefix, StringComparison.Ordinal) && key.Length > Progress.WeaponPrefix.Length)
            {
                into = OwnedWeapons;
                return key.Substring(Progress.WeaponPrefix.Length);
            }
            if (key.StartsWith(Progress.ElementPrefix, StringComparison.Ordinal) && key.Length > Progress.ElementPrefix.Length)
            {
                into = OwnedElements;
                return key.Substring(Progress.ElementPrefix.Length);
            }
            into = null;
            return null;
        }
    }
}
