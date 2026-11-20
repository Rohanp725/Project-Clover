using System;

namespace Clover.Player
{
    /// <summary>
    /// One of the two versions a Shape mark opens (IMPLEMENTATION.md 2.2): its name and words, and what it does on top of
    /// the base spell or instead of it. The Hand chooses which a spell wears (<c>Loadout.Shapes</c>, keyed by the branch's
    /// Learn id); before the Shape is bought the base spell is cast.
    /// </summary>
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

        /// <summary>True when it casts something other than the base spell's form and numbers (a base spell's shape is then cast through its mark, not its element's kit).</summary>
        public bool CastsItsOwn => !string.IsNullOrEmpty(Form) || Spell != null;
    }
}
