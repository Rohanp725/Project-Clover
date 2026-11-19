namespace Clover.Gameplay
{
    /// <summary>
    /// What laid an affliction, as <see cref="AfflictionSet.Lay"/> is told it. It decides whether the laying may start a
    /// reaction (IMPLEMENTATION.md 3.7): everything may, except what a reaction itself laid.
    /// </summary>
    public enum LaySource
    {
        /// <summary>A blow of the weapon, an infused strike among them.</summary>
        Weapon,
        /// <summary>A spell's blow or ground.</summary>
        Spell,
        /// <summary>A mark's effect. An effect run by a reaction that is meant to cascade lays as this, so it may react.</summary>
        Effect,
        /// <summary>A reaction's effect, or the ground a reaction left: it starts no reaction, so cascades stay readable.</summary>
        Reaction,
        /// <summary>The world: water, fire on the ground, an enemy's own attack.</summary>
        World,
    }
}
