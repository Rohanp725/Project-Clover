using Clover.Player;

namespace Clover.UI
{
    /// <summary>One line of the shape screen: a learned Shape mark, the key its choice is kept under, and the shape worn.</summary>
    public struct ShapeRow
    {
        /// <summary>The Shape mark whose two shapes are chosen between.</summary>
        public Upgrade Mark;
        /// <summary>What the choice is kept under in the Hand: the spell branch's Learn id, or the reaction's id.</summary>
        public string Key;
        /// <summary>The spell's or the reaction's name.</summary>
        public string Title;
        /// <summary>True for a reaction's shapes (a duo branch's Shape mark); false for a spell's.</summary>
        public bool Reaction;
        /// <summary>The shape worn now (0 is the first).</summary>
        public int Worn;
        /// <summary>True while the shape is in force: the spell in a slot, or both elements of the reaction carried.</summary>
        public bool InForce;

        public int ShapeCount => Mark != null && Mark.Shapes != null ? Mark.Shapes.Length : 0;
        public ShapeDef WornShape => ShapeCount > 0 ? Mark.Shapes[Worn] : null;
    }
}
