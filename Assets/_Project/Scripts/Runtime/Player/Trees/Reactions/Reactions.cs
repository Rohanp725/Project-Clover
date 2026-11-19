using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// The plain reactions (IMPLEMENTATION.md 2.4; docs/design/steel/pairs_*.md as amended by review_pairs.md): one hidden
    /// mark per pair, never drawn or sold and always in force, whose effects ride <see cref="UpgradeTrigger.Reacted"/>
    /// with the enemy that reacted as their target, and the moments of the conditions they set. What they lay starts no
    /// reaction of its own. All 55, in pair order, split by distance on the ring: <c>Reactions.Near.cs</c> (one and two
    /// apart), <c>Reactions.Three.cs</c>, <c>Reactions.Four.cs</c> and <c>Reactions.Five.cs</c>. Each is the simplest
    /// version that keeps its entry's intent; "NEEDS" names the engine piece a fuller version waits for, and every one is
    /// in the ledger docs/design/steel/content_open.tsv. The look is a placeholder (<see cref="ReactionLook"/>) until the
    /// reactions' own effects are approved.
    /// </summary>
    public static partial class Reactions
    {
        /// <summary>Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.</summary>
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            AddNear(to);
            AddThree(to);
            AddFour(to);
            AddFive(to);
        }

        private static Upgrade Reaction(AfflictionKind one, AfflictionKind other, string words, params UpgradeEffect[] effects) =>
            Reaction(one, other, words, new ReactionDef(), effects);

        private static Upgrade Reaction(AfflictionKind one, AfflictionKind other, string words, float rest, params UpgradeEffect[] effects) =>
            Reaction(one, other, words, new ReactionDef { Rest = rest }, effects);

        // A pair's plain reaction, named and keyed as the pair is, on the two elements whose afflictions they are.
        private static Upgrade Reaction(AfflictionKind one, AfflictionKind other, string words, ReactionDef numbers, params UpgradeEffect[] effects)
        {
            int pair = AfflictionPairs.Index(one, other);
            return new Upgrade
            {
                Id = AfflictionPairs.Id(pair), Name = AfflictionPairs.Name(pair), Kind = UpgradeKind.Reaction, Tree = MarkTree.Hidden,
                Element = ElementOf(AfflictionPairs.First(pair)), Element2 = ElementOf(AfflictionPairs.Second(pair)),
                Description = words, Reaction = numbers, Effects = effects,
            };
        }

        private static string ElementOf(AfflictionKind kind)
        {
            foreach (var info in Elements.All)
                if (info.Affliction == kind) return info.Name;
            return "";
        }
    }
}
