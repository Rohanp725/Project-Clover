using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// An enemy's anchors (<see cref="BodyAnchors"/>) measured when it was built, from its model's mesh and skeleton as
    /// bound. Kept on the enemy because the game cannot measure them itself: the models' meshes are not kept readable
    /// in play. The enemy builder adds it (and Clover > Enemies > Measure Bodies In Scenes brings built scenes up to date).
    /// </summary>
    public class BodyMeasurement : MonoBehaviour
    {
        [SerializeField] private BodyAnchors.Anchors anchors;
        [SerializeField] private float[] widths = new float[0], fronts = new float[0], backs = new float[0];
        [SerializeField] private BodyAnchors.Joints joints;

        public BodyAnchors.Anchors Anchors => anchors;
        public float[] Widths => widths;
        public float[] Fronts => fronts;
        public float[] Backs => backs;
        public BodyAnchors.Joints Joints => joints;

        /// <summary>True when it holds a whole measurement.</summary>
        public bool Valid => anchors.Crown > 0.5f && widths != null && widths.Length == BodyAnchors.Bands
            && fronts != null && fronts.Length == BodyAnchors.Bands && backs != null && backs.Length == BodyAnchors.Bands;

        /// <summary>
        /// Measures the body it is on, where its mesh can be read (the editor, out of play). False, and nothing kept,
        /// for a body without the humanoid skeleton.
        /// </summary>
        public bool Measure()
        {
            var sides = new float[BodyAnchors.Bands];
            var aheads = new float[BodyAnchors.Bands];
            var behinds = new float[BodyAnchors.Bands];
            if (!BodyAnchors.MeasureBody(transform, out var found, sides, aheads, behinds, out var bones)) return false;
            anchors = found;
            widths = sides;
            fronts = aheads;
            backs = behinds;
            joints = bones;
            return true;
        }
    }
}
