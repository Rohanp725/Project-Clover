namespace Clover.Player
{
    /// <summary>
    /// A mark's place in a spell branch (STEEL_AND_ELEMENT.md 5.2, IMPLEMENTATION.md 2.2): four marks that grow in this
    /// order. Its frame and price follow from it: the Learn is a Spell (4), the Empower a Minor (1), the Shape and the
    /// Master Majors (3 each).
    /// </summary>
    public enum BranchPart
    {
        /// <summary>Not in a spell branch: a passive.</summary>
        None,
        /// <summary>Teaches the spell.</summary>
        Learn,
        /// <summary>Stronger, with a shorter cooldown: its <see cref="Upgrade.Empower"/> changes the spell's numbers.</summary>
        Empower,
        /// <summary>Opens two versions of the spell (<see cref="Upgrade.Shapes"/>); the Hand chooses which one each spell wears.</summary>
        Shape,
        /// <summary>A reward for timing the spell well: effects that answer the branch's own spell (<see cref="UpgradeEffect.OwnBranch"/>).</summary>
        Master,
    }
}
