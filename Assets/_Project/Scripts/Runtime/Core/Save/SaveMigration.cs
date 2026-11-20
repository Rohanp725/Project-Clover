using System;
using System.Collections.Generic;
using Clover.Player;
using Clover.Player.Build;

namespace Clover.Core.Save
{
    /// <summary>
    /// How a save comes to be in this build's shape (IMPLEMENTATION.md 5.2): made from the old PlayerPrefs flags the first
    /// time (<see cref="FromPrefs"/>), brought up a version at a time (<see cref="Migrate"/>), and cleared of marks that
    /// were cut (<see cref="RetiredMarks"/>). Nothing here writes to PlayerPrefs, so an older build still runs.
    /// </summary>
    public static class SaveMigration
    {
        /// <summary>The prefix every flag had in PlayerPrefs.</summary>
        public const string PrefsPrefix = "clover.progress.";

        /// <summary>
        /// Marks cut from the trees after a save may have learned them, with what each cost: the one list in
        /// <c>Player/Trees/RetiredMarks.cs</c> (Phase 4 cut Fire's and Lightning's). A cut mark's id is added there and never
        /// used again (8.6).
        /// </summary>
        public static IReadOnlyList<RetiredMark> RetiredMarks => Clover.Player.RetiredMarks.All;

        /// <summary>
        /// The flags the game is known to have set in PlayerPrefs: a flag for every weapon and element (so the owned lists
        /// can be built), the sponsor met, and the slot schedule's elite and god. The world's own keys (doors, ladders,
        /// walls, the land's waking line) are named in the scenes, so they are asked for when first wanted instead
        /// (<see cref="AskPrefs"/>).
        /// </summary>
        public static IEnumerable<string> KnownFlags()
        {
            foreach (var weapon in Weapons.Offered) yield return Progress.WeaponKey(weapon);
            yield return Progress.WeaponKey(Weapons.StormKatanaName);
            yield return Progress.WeaponKey(Weapons.SunSpearName);
            foreach (var element in Elements.All) yield return Progress.ElementKey(element.Name);
            yield return Progress.MetSponsor;
            yield return SlotSchedule.ChampionFlag;
            yield return SlotSchedule.FireGodFlag;
        }

        /// <summary>
        /// A new save made from what PlayerPrefs holds: each of <paramref name="flags"/> set there is set here, a weapon's
        /// or an element's also owned; each is noted as asked. PlayerPrefs is only read.
        /// </summary>
        public static SaveData FromPrefs(IPrefsSource prefs, IEnumerable<string> flags)
        {
            var data = new SaveData();
            if (prefs != null && flags != null)
                foreach (var key in flags) AskPrefs(data, prefs, key);
            data.Version = SaveData.CurrentVersion;
            return data;
        }

        /// <summary>
        /// Whether a flag is set, asking PlayerPrefs the first time it is wanted and copying it in if it was set there.
        /// After that the save alone answers, so a flag forgotten here does not come back from PlayerPrefs.
        /// </summary>
        public static bool AskPrefs(SaveData data, IPrefsSource prefs, string key)
        {
            if (data == null || string.IsNullOrEmpty(key)) return false;
            if (data.HasFlag(key)) return true;
            if (prefs == null || data.PrefsAsked.Contains(key)) return false;
            data.PrefsAsked.Add(key);
            if (prefs.GetInt(PrefsPrefix + key, 0) != 1) return false;
            data.SetFlag(key);
            return true;
        }

        /// <summary>
        /// Brings a save read from a file up to this build: fills what is missing, runs each <see cref="Migrate"/> step
        /// from its version to <see cref="SaveData.CurrentVersion"/>, and takes out any retired mark. Returns the same save.
        /// </summary>
        public static SaveData Upgrade(SaveData data) => Upgrade(data, RetiredMarks);

        /// <summary>As <see cref="Upgrade(SaveData)"/>, with the retired marks given (for tests).</summary>
        public static SaveData Upgrade(SaveData data, IReadOnlyList<RetiredMark> retired)
        {
            if (data == null) return null;
            data.Repair();
            if (data.Version < 1) data.Version = 1;
            while (data.Version < SaveData.CurrentVersion)
            {
                Migrate(data, data.Version);
                data.Version++;
            }
            // Every load, not only on a new version: a mark can be cut without the schema changing, and a second pass
            // finds nothing.
            RemoveRetired(data, retired);
            return data;
        }

        /// <summary>
        /// One step: changes a save of version <paramref name="from"/> into version <paramref name="from"/> + 1. There is
        /// only version 1 so far; the step from 1 to 2 goes here as <c>case 1</c>.
        /// </summary>
        public static void Migrate(SaveData data, int from)
        {
            switch (from)
            {
                default:
                    break;
            }
        }

        /// <summary>
        /// Takes every retired mark out of <see cref="SaveData.Learned"/> and pays its price back in Shards; a retired spell
        /// also leaves any slot of the Hand carried or the kept Hands, and its shape choice is forgotten. Returns the Shards
        /// paid.
        /// </summary>
        public static int RemoveRetired(SaveData data, IReadOnlyList<RetiredMark> retired)
        {
            if (data == null || retired == null || retired.Count == 0) return 0;
            int refunded = 0;
            for (int i = 0; i < retired.Count; i++)
            {
                var mark = retired[i];
                while (data.Learned.Remove(mark.Id)) refunded += mark.Price;
                ClearFromHand(data.Current, mark.Id);
                if (data.SavedHands != null)
                    foreach (var hand in data.SavedHands) ClearFromHand(hand, mark.Id);
            }
            data.Wallet.Shards += refunded;
            return refunded;
        }

        private static void ClearFromHand(Loadout hand, string id)
        {
            if (hand == null) return;
            if (hand.Slots != null)
                for (int i = 0; i < hand.Slots.Length; i++)
                    if (hand.Slots[i] == id) hand.Slots[i] = "";
            hand.Shapes?.RemoveAll(choice => choice != null && choice.Id == id);
            hand.UseEvolved?.RemoveAll(choice => choice != null && choice.Id == id);
        }
    }
}
