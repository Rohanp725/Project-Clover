using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// The names of the small pictures a mark on the chart can carry. Each is painted by code in the editor (UiArt)
    /// under the same name; an upgrade names one of these and the chart tints it.
    /// </summary>
    public static class UpgradeIcons
    {
        // One for each element.
        public const string Flame = "flame";
        public const string Bolt = "bolt";
        public const string Gust = "gust";
        public const string Snowflake = "snowflake";
        public const string Drop = "drop";
        public const string Leaf = "leaf";
        public const string Rock = "rock";
        public const string Anvil = "anvil";
        public const string Sun = "sun";
        public const string Moon = "moon";
        public const string Fang = "fang";
        // What the player does.
        public const string Blade = "blade";
        public const string Fist = "fist";
        public const string Boot = "boot";
        public const string Shield = "shield";
        public const string Flask = "flask";
        public const string Eye = "eye";
        public const string Heart = "heart";
        public const string Star = "star";
        public const string Skull = "skull";
        public const string Hourglass = "hourglass";
        public const string Ring = "ring";
        public const string Arrow = "arrow";
        // The shapes spells take.
        public const string Wave = "wave";
        public const string Orb = "orb";
        public const string Beam = "beam";
        public const string Spike = "spike";
        public const string Wall = "wall";
        public const string Crown = "crown";
        public const string Burst = "burst";
        public const string Comet = "comet";
        public const string Chain = "chain";
        public const string Spiral = "spiral";
        public const string Plus = "plus";
        public const string Target = "target";
        public const string Wing = "wing";
        public const string Tower = "tower";
        public const string Gem = "gem";
        public const string Claw = "claw";
        public const string Hammer = "hammer";

        /// <summary>Every name, in the order the pictures lie in their sheet.</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            Flame, Bolt, Gust, Snowflake, Drop, Leaf, Rock, Anvil, Sun, Moon, Fang,
            Blade, Fist, Boot, Shield, Flask, Eye, Heart, Star, Skull, Hourglass, Ring, Arrow,
            Wave, Orb, Beam, Spike, Wall, Crown, Burst, Comet, Chain, Spiral, Plus, Target, Wing, Tower, Gem, Claw, Hammer,
        };

        public static int IndexOf(string name)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i] == name) return i;
            return -1;
        }
    }
}
