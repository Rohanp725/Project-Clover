using System;

namespace Clover.Player
{
    /// <summary>
    /// One of the two versions a Shape mark opens (IMPLEMENTATION.md 2.2): its name and words, and what it does on top of
    /// the base spell or instead of it. The Hand chooses which a spell wears (<c>Loadout.Shapes</c>, keyed by the branch's
    /// Learn id); before the Shape is bought the base spell is cast.
    /// </summary>
    /// <summary>How an Empower meets the numbers a worn shape sets (<see cref="ShapeDef.Empowered"/>).</summary>
    public enum EmpowerOverShape
    {
        /// <summary>The Empower's numbers are laid over the shape's.</summary>
        Over,
        /// <summary>The numbers the shape sets stand; the Empower changes only the rest (Freezing Fog keeps its 2.5 m).</summary>
        KeepsOwn,
        /// <summary>What the Empower adds to its Learn's number is added to the shape's (Wide Circle's short hang made longer).</summary>
        Adds,
    }

    public class ShapeDef
    {
        public string Name = "";
        public string Description = "";
        /// <summary>A form cast in place of the base spell's (<see cref="SpellForms"/>); empty keeps the base spell's form.</summary>
        public string Form = "";
        /// <summary>The whole spec of the shape, in place of the base spell's numbers; null keeps the base spell's (then <see cref="Change"/>).</summary>
        public SpellSpec Spell;
        /// <summary>Numbers changed on the base spell (or on <see cref="Spell"/>); null for none.</summary>
        public SpellChange Change;
        /// <summary>Changes to the player's numbers while this shape is worn.</summary>
        public StatMod[] Stats = Array.Empty<StatMod>();
        /// <summary>What it does when things happen, while this shape is worn: the shape's own rule, often answering the branch's own spell (<see cref="UpgradeEffect.OwnBranch"/>).</summary>
        public UpgradeEffect[] Effects = Array.Empty<UpgradeEffect>();
        /// <summary>
        /// The places, in its Learn's own effects, of those this shape sets aside while it is worn (the ledger's
        /// shape-sets-aside: Golden Sickle keeps the garden standing, Red-Hot has no hold). Empty: the Learn's effects all run.
        /// </summary>
        public int[] SetsAside = Array.Empty<int>();
        /// <summary>
        /// How its Learn's Empower meets the numbers this shape sets (the ledger's empower-after-shape): laid over them as
        /// written (the old way), the shape's own kept, or the Empower's rise added to the shape's own.
        /// </summary>
        public EmpowerOverShape Empowered = EmpowerOverShape.Over;

        /// <summary>True when the worn shape sets aside the Learn's effect at <paramref name="index"/>.</summary>
        public static bool SetAside(ShapeDef shape, int index) => shape != null && shape.SetsAside != null && Array.IndexOf(shape.SetsAside, index) >= 0;

        /// <summary>True when it casts something other than the base spell's form and numbers (a base spell's shape is then cast through its mark, not its element's kit).</summary>
        public bool CastsItsOwn => !string.IsNullOrEmpty(Form) || Spell != null;
    }
}
