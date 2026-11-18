using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// What the playtest shows as "Not built yet": every god of the full game, with its weapon, and which of them exist.
    /// The Proving Ground's rack sets out an empty, labelled slot for each god's weapon not yet made, and the chart, in
    /// the playtest, draws a slot beside each tree's crown where that god's bough on the weapon tree (the Way) will be.
    /// Taken from docs/design/WORLD.md (the gods' table) and docs/design/steel/README.md ("Every weapon"). The same list,
    /// in words, is in the playtest's READ ME (docs/design/playtest/readme.md).
    /// </summary>
    public static class NotBuiltYet
    {
        public const string Label = "Not built yet";
        /// <summary>The ids of the chart's slots begin with this; nothing with it can be bought.</summary>
        public const string IdPrefix = "notbuilt.";

        public static readonly IReadOnlyList<GodToCome> Gods = new[]
        {
            new GodToCome(Elements.Fire, "Khepharu, the sun-king", Weapons.GreatSpearName, Weapons.SunSpearName, true, Weapons.SunSpearName),
            new GodToCome(Elements.Lightning, "Narukami", Weapons.KatanaName, Weapons.StormKatanaName, true, Weapons.StormKatanaName),
            new GodToCome(Elements.Wind, "Kazehaya", "War Fan (a new weapon)", "The Wind God's Fan", false, ""),
            new GodToCome(Elements.Ice, "The huntress of the snows", Weapons.GreatBowName, "The Huntress's Bow", false, ""),
            new GodToCome(Elements.Water, "The fisher of islands", Weapons.ChainSickleName, "The Fisher's Jawbone (the bone hook)", false, ""),
            new GodToCome(Elements.Life, "Carnwyn", Weapons.ScytheName, "Carnwyn's Golden Scythe", false, ""),
            new GodToCome(Elements.Earth, "Brannoch", Weapons.MassiveSwordName, "Brannoch's Standing Stone", false, ""),
            new GodToCome(Elements.Metal, "The lame smith", Weapons.GreatHammerName, "The Smith's Hammer", false, ""),
            new GodToCome(Elements.Light, "The last champion", "every weapon", "The Champion's Arms (his own version of every weapon)", false, ""),
            new GodToCome(Elements.Dark, "The last champion", "every weapon", "The Champion's Arms (his own version of every weapon)", false, ""),
            new GodToCome(Elements.Poison, "The court alchemist", Weapons.TwinKnivesName, "The Alchemist's Hooks (the hook swords)", false, ""),
        };

        /// <summary>The god whose things sit beside this element's tree.</summary>
        public static GodToCome GodOf(string element)
        {
            foreach (var god in Gods)
                if (god.Element == element) return god;
            return default;
        }

        /// <summary>True for a slot the chart draws in the playtest, not a mark of the game.</summary>
        public static bool IsSlot(Upgrade mark) => mark != null && mark.Id.StartsWith(IdPrefix);

        private static List<Upgrade> slots;

        // Domain reload is off: the slots are placed again from the catalogue as each play finds it.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => slots = null;

        /// <summary>
        /// The chart's slots: one beside each tree's crown, where the god's bough on the Way will be. They are marks in
        /// shape only (drawn as a god's gift, never bought, never in <see cref="Upgrade.All"/>); their words say what
        /// will be there.
        /// </summary>
        public static IReadOnlyList<Upgrade> ChartSlots
        {
            get
            {
                if (slots != null) return slots;
                slots = new List<Upgrade>();
                foreach (var god in Gods)
                {
                    var crown = Gifts.SignatureOf(god.Element);
                    if (crown == null) continue;
                    slots.Add(new Upgrade
                    {
                        Id = IdPrefix + god.Element.ToLowerInvariant() + ".bough",
                        Name = Label + ": " + god.GodWeapon,
                        Element = god.Element,
                        Kind = UpgradeKind.Signature,
                        // Beside the crown, on the side away from the tree's middle.
                        X = crown.X + (crown.X >= 0f ? 1.7f : -1.7f),
                        Y = crown.Y - 0.4f,
                        Icon = crown.Icon,
                        Description = SlotWords(god),
                    });
                }
                return slots;
            }
        }

        public static string SlotWords(GodToCome god) =>
            $"{Label}. Here will be {god.God}'s bough on the {god.WeaponKind}'s Way, the weapon tree: taking {god.Element} from its god opens it and gives {god.GodWeapon}. "
            + (god.WeaponBuilt ? $"The {god.BuiltWeapon} itself is built and stands on the Proving Ground's rack; its bough and moves are not. " : "Neither the weapon nor its moves are made yet. ")
            + (god.GodBuilt ? "" : $"{god.God} is not built yet either. ")
            + "The Way arrives in phase 5 of the playtest.";

        /// <summary>What a god's gift says of where it comes from, in the playtest where it can be bought.</summary>
        public static string GiftSource(string element)
        {
            var god = GodOf(element);
            if (string.IsNullOrEmpty(god.God)) return "Given by a god.";
            return god.GodBuilt ? $"Given by {god.God} when taken." : $"Given by {god.God}, who is {Label.ToLowerInvariant()}.";
        }
    }
}
