using UnityEngine;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The menus' text sizes (ui_rest_fire_plan.md 3.5.8 and 4.1): 100%, 125% and 150%. Every size in Tokens.uss is a
    /// variable; the setting puts <c>size-125</c> or <c>size-150</c> on each panel's root, which redefines them.
    /// </summary>
    public static class TextSizes
    {
        public const int Normal = 100;
        public const int Larger = 125;
        public const int Largest = 150;

        /// <summary>The three sizes in the order L3 steps through them.</summary>
        public static readonly int[] Steps = { Normal, Larger, Largest };

        /// <summary>The next size after this one: 100, 125, 150, then back to 100.</summary>
        public static int Next(int size)
        {
            int at = Normalise(size);
            for (int i = 0; i < Steps.Length; i++)
                if (Steps[i] == at) return Steps[(i + 1) % Steps.Length];
            return Normal;
        }

        /// <summary>The nearest of the three sizes (a saved value from a newer build, or none, comes back as one of them).</summary>
        public static int Normalise(int size) => size >= Largest ? Largest : size >= Larger ? Larger : Normal;

        /// <summary>The root's class for a size: "" at 100%.</summary>
        public static string ClassOf(int size) => Normalise(size) switch
        {
            Larger => "size-125",
            Largest => "size-150",
            _ => "",
        };

        /// <summary>The words for the footer: "Text size 125%".</summary>
        public static string Words(int size) => "Text size " + Normalise(size) + "%";
    }

    /// <summary>The text size saved with the settings on this machine (as the controls are). Read by every menu.</summary>
    public static class TextSizeSetting
    {
        public const string PrefsKey = "clover.ui.textsize";

        private static int cached;

        // Domain reload is off: read the saved size again in a new play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => cached = 0;

        /// <summary>The size worn now: 100, 125 or 150.</summary>
        public static int Current
        {
            get
            {
                if (cached == 0) cached = TextSizes.Normalise(PlayerPrefs.GetInt(PrefsKey, TextSizes.Normal));
                return cached;
            }
            set
            {
                cached = TextSizes.Normalise(value);
                PlayerPrefs.SetInt(PrefsKey, cached);
                PlayerPrefs.Save();
            }
        }
    }
}
