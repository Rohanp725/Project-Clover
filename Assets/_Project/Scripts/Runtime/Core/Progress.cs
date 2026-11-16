using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// What stays with the player from run to run and from one sitting to the next: which weapons and elements a run
    /// may begin with, and whether the sponsor has already said its first words. Kept in the machine's own settings
    /// store, a flag at a time.
    /// </summary>
    public static class Progress
    {
        public const string MetSponsor = "met.sponsor";

        private const string Prefix = "clover.progress.";

        public static string WeaponKey(string weaponName) => "weapon." + weaponName;
        public static string ElementKey(string elementName) => "element." + elementName;

        public static bool Has(string key) => PlayerPrefs.GetInt(Prefix + key, 0) == 1;

        public static void Unlock(string key)
        {
            if (Has(key)) return;
            PlayerPrefs.SetInt(Prefix + key, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Forgets one thing; for starting over, and for tests.</summary>
        public static void Forget(string key)
        {
            PlayerPrefs.DeleteKey(Prefix + key);
            PlayerPrefs.Save();
        }
    }
}
