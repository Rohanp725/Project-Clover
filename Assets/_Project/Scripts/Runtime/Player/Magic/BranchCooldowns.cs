namespace Clover.Player.Magic
{
    /// <summary>
    /// The three actions a Master (or any mark) has over the spell clocks (IMPLEMENTATION.md 3.1):
    /// <see cref="UpgradeAction.CutCooldowns"/>, <see cref="UpgradeAction.ResetCooldown"/> and
    /// <see cref="UpgradeAction.InstantNext"/>. Plain rules over <see cref="SpellCooldowns"/> and <see cref="SpellSlots"/>,
    /// so they are tested without a scene; <see cref="PlayerSpells.Answer"/> hands them its own clocks and slots.
    /// </summary>
    public static class BranchCooldowns
    {
        /// <summary>The branch an effect acts on: the one it names, else its own mark's.</summary>
        public static string BranchOf(UpgradeEffect effect) =>
            effect == null ? "" : !string.IsNullOrEmpty(effect.Branch) ? effect.Branch
            : effect.Owner == null ? "" : !string.IsNullOrEmpty(effect.Owner.Branch) ? effect.Owner.Branch : effect.Owner.Spell != null ? effect.Owner.Id : "";

        /// <summary>True for the actions this class does.</summary>
        public static bool Handles(UpgradeAction action) =>
            action == UpgradeAction.CutCooldowns || action == UpgradeAction.ResetCooldown || action == UpgradeAction.InstantNext || action == UpgradeAction.KeepWaitShare;

        /// <summary>
        /// Does the effect's action on the clocks. Returns the slot acted on (-1 for every clock, or for none: a branch whose
        /// spell sits in no slot is left alone).
        /// </summary>
        public static int Answer(UpgradeEffect effect, SpellCooldowns clocks, SpellSlots slots) => Answer(effect, BranchOf(effect), clocks, slots);

        /// <summary>The same with the branch given (a test's effect has no owner).</summary>
        public static int Answer(UpgradeEffect effect, string branch, SpellCooldowns clocks, SpellSlots slots)
        {
            if (effect == null || clocks == null) return -1;
            switch (effect.Do)
            {
                case UpgradeAction.CutCooldowns:
                    clocks.Cut(effect.Amount);
                    return -1;
                case UpgradeAction.ResetCooldown:
                {
                    int slot = slots != null ? slots.SlotOf(branch) : -1;
                    if (slot >= 0) clocks.Reset(slot);
                    return slot;
                }
                case UpgradeAction.KeepWaitShare:
                {
                    // Its own spell's wait alone, not every clock.
                    int slot = slots != null ? slots.SlotOf(branch) : -1;
                    if (slot >= 0) clocks.KeepShare(slot, effect.Amount > 0f ? effect.Amount : 0.5f);
                    return slot;
                }
                case UpgradeAction.InstantNext:
                {
                    int slot = slots != null ? slots.SlotOf(branch) : -1;
                    if (slot >= 0) clocks.InstantNext(slot, effect.Seconds);
                    return slot;
                }
                default:
                    return -1;
            }
        }
    }
}
