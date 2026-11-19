namespace Clover.Player.Swings
{
    /// <summary>
    /// How one weapon's swing trail is shaped, whatever the element on it: which part of its blade line the trail
    /// covers, how wide and how long it lasts, the tip speeds between which it fades in, and whether its blows that
    /// land on the ground throw the element there. Read from <see cref="SwingShapes"/>.
    /// </summary>
    public class SwingShape
    {
        /// <summary>The weapon's name, for the screen and the tests.</summary>
        public string Weapon = "";
        /// <summary>The share of the blade line, from its base (0) to its tip (1), the trail covers. Past 1 it reaches beyond the point; below 0, down the haft.</summary>
        public float SweepFrom = 0.3f, SweepTo = 1.02f;
        /// <summary>What every width is multiplied by: a strand's, a sheath's, a thorn's.</summary>
        public float Width = 1f;
        /// <summary>What every trail's seconds are multiplied by.</summary>
        public float Life = 1f;
        /// <summary>Tip speeds in metres a second: below the least nothing is left behind, at the full the trail is whole.</summary>
        public float LeastSpeed = 2f, FullSpeed = 7f;
        /// <summary>Metres across a line drawn about a point that thrusts or punches (the spear's tip, a fist, the chain's weight).</summary>
        public float StrandWidth = 0.3f;
        /// <summary>True when its blows that strike the ground (a slam, a buried overhead) throw the element out where they land.</summary>
        public bool GroundImpact;
        /// <summary>What the shape is for, in a line.</summary>
        public string Note = "";
    }
}
