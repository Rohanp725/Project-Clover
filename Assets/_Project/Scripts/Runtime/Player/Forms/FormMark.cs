namespace Clover.Player.Forms
{
    /// <summary>
    /// The flat shapes of light a spell form can lay on the ground or hang in the air through <see cref="FormFx"/>.
    /// The builder makes one material for each, in this order.
    /// </summary>
    public enum FormMark
    {
        /// <summary>A hard ring.</summary>
        Ring,
        /// <summary>A disc with a bold rim and a faint inside: where something will land.</summary>
        Disc,
        /// <summary>An eight-pointed burst with a white heart: the frame of an impact.</summary>
        Star,
        /// <summary>Two cuts crossed.</summary>
        Cross,
        /// <summary>Cracks running out from a point.</summary>
        Crack,
        /// <summary>A sign of rings and triangles: a spell forming.</summary>
        Sigil,
        /// <summary>A beam with a white core, running along the strip.</summary>
        Beam,
        /// <summary>A lane of arrowheads, running along the strip: the path something will take.</summary>
        Lane,
        /// <summary>A streak with a white core, pointed at both ends, running along the strip: a cut of light.</summary>
        Streak,
        /// <summary>A dark soft stain: what a blast leaves on the ground.</summary>
        Scorch,
        /// <summary>Four brackets round something.</summary>
        Brackets,
    }
}
