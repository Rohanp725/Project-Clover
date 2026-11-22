using System.Collections.Generic;
using System.Text.RegularExpressions;
using Clover.Player;
using Clover.Player.Trees.Ways;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The Loadout's own words (ui_rest_fire_plan.md 4.4): the role names and what each role is for, the keywords' one-line
    /// meanings, and for each Way choice the few words a card shows, what turning it off takes away, its press and its
    /// icon. Written at an 8th-grade level, each sentence 15 words or fewer. Only the Katana's Way and the Body are
    /// written; any other weapon's marks fall back to their own first sentence (ledgered in content_open.tsv, ui-words).
    /// </summary>
    public static class LoadoutWords
    {
        /// <summary>A Way mark's card words: its short line, what you lose when it is turned off, its cost and press, its icon.</summary>
        public readonly struct Entry
        {
            public readonly string Short, Gives, Cost, Press, Icon, Change;

            public Entry(string shortLine, string gives, string icon, string cost = "", string press = "", string change = "")
            {
                Short = shortLine;
                Gives = gives;
                Icon = icon;
                Cost = cost;
                Press = press;
                Change = change;
            }
        }

        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>
        {
            // The Katana's boughs (aspects): the card's line and what choosing it changes.
            [KatanaWay.Sheath] = new Entry("Every combo starts with a draw", "the hard draw that opens every combo", IconShapes.Sheath,
                change: "Your combo becomes a draw, a return cut, then the sheathe."),
            [KatanaWay.Stream] = new Entry("Long combos that flow", "the five-cut combo you can dodge through", IconShapes.Wave,
                change: "Your combo grows to five cuts, and a dodge keeps it going."),
            [KatanaWay.Answer] = new Entry("A perfect guard lets you answer", "the counter after a perfect guard", IconShapes.Shield,
                change: "A perfect guard gives you a moment to answer. Light cuts hit a little softer."),
            [KatanaWay.Storm] = new Entry("Narukami's own sword", "Narukami's sword and its Thunder Draw", IconShapes.Bolt,
                change: "You carry Narukami's sword. It reaches further and has his combo."),
            // The keystones.
            [KatanaWay.TheCutLandsAtTheClick] = new Entry("A long held draw", "the long held draw that cuts through guards", IconShapes.Click,
                press: "Hold {LightAttack}, then let go"),
            [KatanaWay.AHundredDays] = new Entry("Quick draws, again and again", "the quick draws that leave cuts in the air", IconShapes.Shrine,
                press: "Press {LightAttack} again and again"),
            [KatanaWay.NoLastCut] = new Entry("The combo loops and grows", "the combo that loops and hits harder", IconShapes.Ring),
            [KatanaWay.TheWaterHasNoShape] = new Entry("Every slide is a cut", "the slides that cut and count as cuts", IconShapes.Drop,
                press: "Press {Dodge} during your combo"),
            [KatanaWay.KiriOtoshi] = new Entry("Your answer comes on its own", "the riposte that comes on its own", IconShapes.Star),
            [KatanaWay.TheFirstSword] = new Entry("One cut through a red attack", "the cut that stops time on a red attack", IconShapes.Crown),
            [KatanaWay.Raikiri] = new Entry("Lightning down a long line", "the lightning cut down a long line", IconShapes.Bolt,
                press: "Hold {WeaponSkill} past the Thunder Draw"),
            [KatanaWay.TheDrumRing] = new Entry("Seven drums call the storm", "the ring of bolts when seven drums are beaten", IconShapes.Crown),
            // The Body's two bearings.
            [BodyWay.EnduringBody] = new Entry("Hits push you less", "the toughness that keeps you standing", IconShapes.Stone),
            [BodyWay.LightBody] = new Entry("Quicker dodges", "the second dodge for half the stamina", IconShapes.Feather,
                press: "Press {Dodge} just as a dodge ends"),
            // The technique.
            [KatanaWay.BeforeTheThunder] = new Entry("A draw that breaks attacks", "", IconShapes.Bolt, press: "Press {LightAttack} and {HeavyAttack} together"),
        };

        /// <summary>True when the mark has words written here.</summary>
        public static bool Has(string id) => id != null && entries.ContainsKey(id);

        /// <summary>The mark's words, or none (every field "").</summary>
        public static Entry For(string id) => id != null && entries.TryGetValue(id, out var entry) ? entry : new Entry("", "", "");

        // ---- Roles.

        /// <summary>The five roles in the strip's order.</summary>
        public static readonly SpellRole[] Roles = { SpellRole.Opener, SpellRole.Breaker, SpellRole.Detonator, SpellRole.Counter, SpellRole.Movement };

        /// <summary>The role as a noun in a sentence: "opener", "movement spell".</summary>
        public static string RoleNoun(SpellRole role) => role switch
        {
            SpellRole.Opener => "opener",
            SpellRole.Breaker => "breaker",
            SpellRole.Detonator => "detonator",
            SpellRole.Counter => "counter",
            SpellRole.Movement => "movement spell",
            _ => "",
        };

        /// <summary>The role's name as a label: "Opener".</summary>
        public static string RoleName(SpellRole role) => role == SpellRole.None ? "" : role.ToString();

        /// <summary>The role's letter in a folded summary: O, B, D, C, M.</summary>
        public static string RoleLetter(SpellRole role) => role == SpellRole.None ? "" : role.ToString().Substring(0, 1);

        /// <summary>What a role is for, one line.</summary>
        public static string RoleMeaning(SpellRole role) => role switch
        {
            SpellRole.Opener => "Lays your element on them to start a fight.",
            SpellRole.Breaker => "Breaks their guard. It fills the stagger bar fast.",
            SpellRole.Detonator => "Spends the element you laid for a big hit.",
            SpellRole.Counter => "Answers an attack you saw coming.",
            SpellRole.Movement => "Moves you fast, into a fight or out of it.",
            _ => "",
        };

        /// <summary>The role-strip's line for the roles missing ("No movement spell", "No opener or counter"); "" when none is missing.</summary>
        public static string Missing(int[] counts)
        {
            string first = "", second = "";
            int missing = 0;
            foreach (var role in Roles)
            {
                if (counts[(int)role] > 0) continue;
                missing++;
                if (missing == 1) first = RoleNoun(role);
                else if (missing == 2) second = RoleNoun(role);
            }
            if (missing == 0) return "";
            if (missing == Roles.Length) return "No spells yet";
            if (missing == 1) return "No " + first;
            // Three or more names run past the strip; the cells outlined in red name them.
            return missing == 2 ? "No " + first + " or " + second : missing + " roles missing";
        }

        // ---- Keywords: the first one in a pane's words gets its meaning at the pane's foot (4.4).

        private static readonly string[] keywords = { "draw", "perfect guard", "stagger", "Burn", "Static", "riposte" };
        private static readonly string[] meanings =
        {
            "a cut made straight from the sheath. It hits harder.",
            "a guard pressed just as the hit lands.",
            "when the bar fills, the enemy reels and is open.",
            "fire that eats at them until it is spent.",
            "a charge that builds on them until it bursts.",
            "a quick cut right after you block.",
        };
        private static readonly Regex[] keywordPatterns = MakePatterns();

        private static Regex[] MakePatterns()
        {
            var made = new Regex[keywords.Length];
            for (int i = 0; i < keywords.Length; i++) made[i] = new Regex(@"\b" + keywords[i] + @"(s|n)?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return made;
        }

        /// <summary>The first keyword the words use and its meaning; false when they use none.</summary>
        public static bool FindKeyword(string words, out string keyword, out string meaning)
        {
            keyword = meaning = "";
            if (string.IsNullOrEmpty(words)) return false;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (!keywordPatterns[i].IsMatch(words)) continue;
                keyword = char.ToUpperInvariant(keywords[i][0]) + keywords[i].Substring(1);
                meaning = meanings[i];
                return true;
            }
            return false;
        }

        // ---- Small things.

        private static readonly string[] numerals = { "", "I", "II", "III", "IV", "V" };

        /// <summary>"Temper II"; "No Temper yet" at rank 0.</summary>
        public static string TemperName(int rank) => rank <= 0 ? "No Temper yet" : "Temper " + numerals[System.Math.Min(rank, numerals.Length - 1)];

        /// <summary>The button word for a slot (0 West to 3 South).</summary>
        public static readonly string[] SlotWords = { "West", "North", "East", "South" };

        /// <summary>The action that casts a slot, for its glyph (the Spells map's), or the surge's.</summary>
        public static readonly string[] SlotActions = { "SpellWest", "SpellNorth", "SpellEast", "SpellSouth", "Surge" };

        /// <summary>"Bought for 1 Memory", "Bought for 4 Memories".</summary>
        public static string Memories(int amount) => amount == 1 ? "1 Memory" : amount + " Memories";

        /// <summary>The first sentence of a text, for a card's line where no short words are written.</summary>
        public static string FirstSentence(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            int stop = text.IndexOf(". ", System.StringComparison.Ordinal);
            return stop < 0 ? text.TrimEnd('.') : text.Substring(0, stop);
        }
    }
}
