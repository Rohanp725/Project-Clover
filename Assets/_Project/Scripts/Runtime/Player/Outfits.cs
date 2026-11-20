using System;
using Clover.Core;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Which outfit the player wears where. Each land dresses the player in its own culture's clothes (the owner's
    /// decision of 2026-11-18); a scene outside every land, such as the Hub, keeps whatever was worn last. A chosen
    /// outfit, once the wardrobe exists, overrides the land. The chosen and the last outfit are kept in PlayerPrefs; a
    /// sandboxed play (probes) keeps them under keys of its own so it never touches what the owner chose.
    /// </summary>
    public static class Outfits
    {
        public const string Ronin = "ronin";
        public const string Fire = "fire";
        public const string Ice = "ice";
        public const string Water = "water";
        public const string Metal = "metal";
        public const string Poison = "poison";
        /// <summary>Earth and Life share one land and one outfit.</summary>
        public const string Celtic = "celtic";
        /// <summary>The last champion's land, Light and Dark.</summary>
        public const string Champion = "champion";
        /// <summary>
        /// The generated ronin (ComfyUI and TRELLIS.2, bound to the same rig), put in on 2026-11-20 as a trial to judge in
        /// play beside the current ronin. No land wears it: it is on only while it is the chosen outfit.
        /// </summary>
        public const string RoninGen = "ronin_gen";

        /// <summary>
        /// Every outfit the game knows, the default first. An outfit whose model is not in the project yet is left out of
        /// the built set, and <see cref="Choose"/> passes over it.
        /// </summary>
        public static readonly string[] All = { Ronin, Fire, Ice, Water, Metal, Poison, Celtic, Champion, RoninGen };

        /// <summary>
        /// Outfits on trial: worn only when chosen, and never kept as the last outfit, so taking one off brings back
        /// what the lands and the last scene would have put on.
        /// </summary>
        public static readonly string[] Trials = { RoninGen };

        public static bool IsTrial(string id) => Array.IndexOf(Trials, id) >= 0;

        /// <summary>
        /// The playtest's switch for the trial outfit: what the chosen outfit becomes when it is pressed. On takes the
        /// place of any other choice; off leaves no choice, so the lands decide again.
        /// </summary>
        public static string ToggleTrial(string chosen) => chosen == RoninGen ? "" : RoninGen;

        public const string ChosenPrefsKey = "clover.outfit.chosen";
        public const string ChosenSandboxKey = "clover.outfit.chosen.sandbox";
        public const string LastPrefsKey = "clover.outfit.last";
        public const string LastSandboxKey = "clover.outfit.last.sandbox";

        public static string ChosenKey => Progress.Sandboxed ? ChosenSandboxKey : ChosenPrefsKey;
        public static string LastKey => Progress.Sandboxed ? LastSandboxKey : LastPrefsKey;

        /// <summary>The land a scene belongs to, or null for a scene outside every land.</summary>
        public static string LandOf(string sceneName)
        {
            switch (sceneName)
            {
                case "SunCity":
                case "SunCourt":
                    return Elements.Fire;
                case "Castle":
                case "Summit":
                    return Elements.Lightning;
                default:
                    return null;
            }
        }

        /// <summary>The outfit a land dresses the player in, or null for a land with none of its own yet.</summary>
        public static string ForLand(string land)
        {
            switch (land)
            {
                // The ronin is the Lightning and Wind lands' own.
                case Elements.Lightning:
                case Elements.Wind:
                    return Ronin;
                case Elements.Fire:
                    return Fire;
                case Elements.Ice:
                    return Ice;
                case Elements.Water:
                    return Water;
                case Elements.Metal:
                    return Metal;
                case Elements.Poison:
                    return Poison;
                case Elements.Earth:
                case Elements.Life:
                    return Celtic;
                case Elements.Light:
                case Elements.Dark:
                    return Champion;
                default:
                    return null;
            }
        }

        /// <summary>The scene's own outfit, or null where the last one stays on.</summary>
        public static string ForScene(string sceneName) => ForLand(LandOf(sceneName));

        /// <summary>
        /// What to wear in a scene: the chosen outfit when there is one the game knows, else the land's, else the last
        /// one worn, else the ronin. <paramref name="known"/> says whether an outfit can be worn; null means all can.
        /// </summary>
        public static string Choose(string sceneName, string chosen, string last, Func<string, bool> known = null)
        {
            bool Wearable(string id) => !string.IsNullOrEmpty(id) && (known == null || known(id));
            if (Wearable(chosen)) return chosen;
            string own = ForScene(sceneName);
            if (Wearable(own)) return own;
            if (Wearable(last)) return last;
            return Ronin;
        }

        /// <summary>The outfit the player chose to wear everywhere, or empty when the lands decide.</summary>
        public static string Chosen
        {
            get => PlayerPrefs.GetString(ChosenKey, "");
            set
            {
                if (string.IsNullOrEmpty(value)) PlayerPrefs.DeleteKey(ChosenKey);
                else PlayerPrefs.SetString(ChosenKey, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>The outfit worn last, which a scene outside every land keeps on. A trial outfit is never kept as it.</summary>
        public static string Last
        {
            get => PlayerPrefs.GetString(LastKey, "");
            set
            {
                if (string.IsNullOrEmpty(value) || IsTrial(value) || PlayerPrefs.GetString(LastKey, "") == value) return;
                PlayerPrefs.SetString(LastKey, value);
                PlayerPrefs.Save();
            }
        }
    }
}
