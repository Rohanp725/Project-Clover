using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Growth
{
    /// <summary>What a defeat pays Memories as (way_plan.md 0.3): the rank of who was beaten.</summary>
    public enum DefeatTier
    {
        /// <summary>An ordinary enemy: the first of its kind pays, never another.</summary>
        Common,
        /// <summary>A named fighter on a side path.</summary>
        Named,
        /// <summary>An elite, the teacher of a gated technique.</summary>
        Elite,
        /// <summary>A god.</summary>
        God,
        /// <summary>The Teacher, the champion's land's elite.</summary>
        Teacher,
        /// <summary>The last champion.</summary>
        LastChampion,
    }

    /// <summary>
    /// What the world pays (IMPLEMENTATION.md 3.12, task 5.6): Memories for the first defeat of each kind of enemy, for
    /// elites, named fighters, gods, the Teacher and the last champion, and for Memories found lying in the world; Shards
    /// per kill by tier. Memories cannot be farmed (docs/GAME.md): every Memory is paid once, under a key kept in the
    /// save (<c>SaveData.KindsDefeated</c> for defeats, <c>SaveData.Found</c> for what is found), so a second defeat of
    /// the same kind, the same elite or the same god after a rest pays nothing.
    ///
    /// The numbers are the owner's of 2026-11-19 (way_plan.md 0.3, question G6: a god 8, an elite 4, each new kind 1)
    /// and the plan's proposal for the rest (a named fighter 2, the Teacher 4, the last champion 12); public fields, so
    /// play can tune them. One payment per defeat: an elite, a god or a named fighter is paid as what it is, not also as
    /// a new kind (docs/design/steel/phase5_questions.md). Plain: whoever hears of the defeat asks this and puts the
    /// result in the wallet.
    /// </summary>
    public sealed class Income
    {
        /// <summary>The key a god's fall is paid under: its progress flag, <c>god.fire</c>, as <c>SlotSchedule.FireGodFlag</c> is.</summary>
        public const string GodPrefix = "god.";

        public int FirstOfAKind = 1;
        public int Named = 2;
        public int Elite = 4;
        public int God = 8;
        public int Teacher = 4;
        public int LastChampion = 12;
        /// <summary>Memories found lying in a land of one element, and in a land of two (way_plan.md 0.3).</summary>
        public int FoundInSoloLand = 3;
        public int FoundInDuoLand = 5;

        /// <summary>Shards per kill by rank, below tier 3 (an enemy pays once a visit) and from tier 3 on (IMPLEMENTATION.md 3.12).</summary>
        public int ShardsCommon = 1, ShardsElite = 3, ShardsGod = 5;
        public int ShardsCommonLate = 2, ShardsEliteLate = 5, ShardsGodLate = 8;
        /// <summary>The land tier from which kills pay the later Shards.</summary>
        public int LateTier = 3;

        /// <summary>The Memories a defeat of this rank is worth the first time.</summary>
        public int MemoriesFor(DefeatTier tier)
        {
            switch (tier)
            {
                case DefeatTier.Named: return Named;
                case DefeatTier.Elite: return Elite;
                case DefeatTier.God: return God;
                case DefeatTier.Teacher: return Teacher;
                case DefeatTier.LastChampion: return LastChampion;
                default: return FirstOfAKind;
            }
        }

        /// <summary>
        /// A defeat: the Memories it pays, and its key noted in <paramref name="paid"/> so it never pays again. Nothing for
        /// a key already paid, or for no key at all.
        /// </summary>
        public int Defeat(string key, DefeatTier tier, ICollection<string> paid)
        {
            if (string.IsNullOrEmpty(key) || paid == null || paid.Contains(key)) return 0;
            paid.Add(key);
            return MemoriesFor(tier);
        }

        /// <summary>
        /// The defeat of an enemy kind, paid under its name and by its rank (<see cref="TierOf"/>). A god's body pays nothing
        /// here: a god fights in more than one body and phase, so it is paid once, when it falls (<see cref="GodFelled"/>).
        /// </summary>
        public int Defeat(EnemyKind kind, ICollection<string> paid) =>
            kind == null || kind.IsGod ? 0 : Defeat(KeyOf(kind), TierOf(kind), paid);

        /// <summary>A god's fall, paid once under its flag (<c>god.fire</c>).</summary>
        public int GodFelled(string element, ICollection<string> paid) =>
            string.IsNullOrEmpty(element) ? 0 : Defeat(GodKey(element), DefeatTier.God, paid);

        /// <summary>Memories found in the world, under the pickup's own key, once: <paramref name="duoLand"/> for a land of two elements.</summary>
        public int Found(string key, bool duoLand, ICollection<string> found)
        {
            if (string.IsNullOrEmpty(key) || found == null || found.Contains(key)) return 0;
            found.Add(key);
            return duoLand ? FoundInDuoLand : FoundInSoloLand;
        }

        /// <summary>The Shards one kill pays by the enemy's rank and the land's tier.</summary>
        public int ShardsFor(DefeatTier tier, int landTier)
        {
            bool late = landTier >= LateTier;
            switch (tier)
            {
                case DefeatTier.God:
                case DefeatTier.LastChampion:
                    return late ? ShardsGodLate : ShardsGod;
                case DefeatTier.Elite:
                case DefeatTier.Teacher:
                case DefeatTier.Named:
                    return late ? ShardsEliteLate : ShardsElite;
                default:
                    return late ? ShardsCommonLate : ShardsCommon;
            }
        }

        /// <summary>
        /// An enemy kind's rank for the Memories: a god's body is a god, an elite an elite, anyone else an ordinary enemy.
        /// A named fighter is named by its kind once the first is built (no kind says so yet).
        /// </summary>
        public static DefeatTier TierOf(EnemyKind kind)
        {
            // NEEDS: a field on EnemyKind for a named fighter, the Teacher and the last champion, when the first of each is built.
            if (kind == null) return DefeatTier.Common;
            if (kind.IsGod) return DefeatTier.God;
            if (kind.IsElite) return DefeatTier.Elite;
            return DefeatTier.Common;
        }

        /// <summary>The key a kind is paid under: its name, which every kind of one sort shares (each rank of the Gilded is its own).</summary>
        public static string KeyOf(EnemyKind kind) => kind == null ? "" : kind.Name ?? "";

        /// <summary>The key a god's fall is paid under, from its element: <c>god.fire</c>.</summary>
        public static string GodKey(string element) => GodPrefix + (element ?? "").ToLowerInvariant();
    }
}
