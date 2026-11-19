namespace Clover.Player.Swings
{
    /// <summary>
    /// A blade line made of the body, for bare hands and the Armament: from one bone to the next and on past it by a
    /// fist's or a foot's length (the forearm to the knuckles, the shin to the toes).
    /// </summary>
    public readonly struct LimbLine
    {
        public readonly string Part;
        public readonly string Bone;
        public readonly string TipBone;
        /// <summary>Metres the line reaches past the tip bone, along the limb.</summary>
        public readonly float Beyond;
        public readonly float Thickness;

        public LimbLine(string part, string bone, string tipBone, float beyond, float thickness)
        {
            Part = part;
            Bone = bone;
            TipBone = tipBone;
            Beyond = beyond;
            Thickness = thickness;
        }
    }
}
