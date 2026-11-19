namespace Clover.Player.Build
{
    /// <summary>
    /// What an element altar of the Proving Ground does to the attunements (IMPLEMENTATION.md, Phase 2, task 2.4), kept
    /// to one rule a tester can learn from the prompt: with nothing carried, the altar's element is carried; with one
    /// carried, another altar's element is carried second, and the same altar again carries it all in; with two carried
    /// (or all in), the altar starts again with its element alone. Plain C#, so the rule is tested without a scene.
    /// </summary>
    public static class AltarChoice
    {
        /// <summary>The attunements after using the altar of <paramref name="altar"/> ("" in the second for one element).</summary>
        public static void Next(string first, string second, string altar, out string newFirst, out string newSecond)
        {
            first ??= "";
            second ??= "";
            altar ??= "";
            if (first.Length == 0 || second.Length > 0)
            {
                newFirst = altar;
                newSecond = "";
                return;
            }
            newFirst = first;
            newSecond = altar;
        }

        /// <summary>What the altar offers before it is used, as its prompt says it: "Carry Ice", "Carry Ice as second", "Carry Fire all in", "Carry Ice alone".</summary>
        public static string Prompt(string first, string second, string altar)
        {
            Next(first, second, altar, out string newFirst, out string newSecond);
            if (newSecond.Length == 0) return string.IsNullOrEmpty(first) ? "Carry " + altar : "Carry " + altar + " alone";
            return newSecond == newFirst ? "Carry " + altar + " all in" : "Carry " + altar + " as second";
        }

        /// <summary>What is said once it is used: "Fire", "Fire, then Lightning", "Fire all in".</summary>
        public static string Said(string first, string second)
        {
            first ??= "";
            second ??= "";
            if (second.Length == 0) return first;
            return second == first ? first + " all in" : first + ", then " + second;
        }
    }
}
