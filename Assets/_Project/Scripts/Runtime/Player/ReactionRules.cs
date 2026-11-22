namespace Clover.Player
{
    /// <summary>Plain rules about a reaction's own effects, apart from the runner so they can be checked without a scene.</summary>
    public static class ReactionRules
    {
        /// <summary>
        /// True for an effect that strikes: what a reaction struck again does once more (Strikes Twice, Spreading Water). What
        /// it lays, sets, spends or moves is done only the first time.
        /// </summary>
        public static bool StrikesAgain(UpgradeAction action) =>
            action == UpgradeAction.Burst || action == UpgradeAction.Lane || action == UpgradeAction.Arc || action == UpgradeAction.Falling
            || action == UpgradeAction.Projectile || action == UpgradeAction.Cone || action == UpgradeAction.Helper;

        /// <summary>
        /// What the marks' <see cref="UpgradeAction.BoostReaction"/> effects in <paramref name="effects"/> make of the reaction
        /// <paramref name="reaction"/> as it fires on an enemy carrying <paramref name="on"/>: its own damage multiplied (1:
        /// unchanged), and <paramref name="spares"/> true when it spends nothing (Goes Up: at full depth of poison, Fire
        /// Medicine twice as hard and spending nothing; the ledger's reaction-numbers-when). Each effect's tests are asked of
        /// the enemy now, before the reaction's own effects spend anything.
        /// </summary>
        public static float Boost(System.Collections.Generic.IReadOnlyList<UpgradeEffect> effects, string reaction, Clover.Gameplay.AfflictionSet on, out bool spares)
        {
            spares = false;
            float scale = 1f;
            if (effects == null || on == null) return scale;
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null || effect.Do != UpgradeAction.BoostReaction) continue;
                if (!string.IsNullOrEmpty(effect.Reaction) && effect.Reaction != reaction) continue;
                if (effect.Needs != Clover.Gameplay.AfflictionKind.None)
                {
                    if (!on.Has(effect.Needs)) continue;
                    if (effect.NeedsFull && on.Stacks(effect.Needs) < on.DepthOf(effect.Needs)) continue;
                    if (effect.NeedsAtLeast > 0 && on.Stacks(effect.Needs) < effect.NeedsAtLeast) continue;
                }
                if (effect.Amount > 0f) scale *= effect.Amount;
                spares |= effect.SparesSpend;
            }
            return scale;
        }
    }
}
