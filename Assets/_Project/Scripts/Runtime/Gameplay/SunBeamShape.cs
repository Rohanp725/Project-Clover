using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// How a beam of reflected sun is drawn: a hot streak of light at chest height from the mirror to where it ends,
    /// and the lane it falls on lit pale on the ground under it. Two flat quads of light the builder makes; this only
    /// lays them along the beam and sets how bright they are.
    /// </summary>
    public class SunBeamShape
    {
        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private readonly Transform beam, lane;
        private readonly Renderer beamRenderer, laneRenderer;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

        public Color BeamColour = new Color(4f, 3.3f, 1.8f, 1f);
        public Color LaneColour = new Color(1.4f, 1.2f, 0.75f, 0.55f);

        public SunBeamShape(Transform beamQuad, Transform laneQuad)
        {
            beam = beamQuad;
            lane = laneQuad;
            beamRenderer = beam != null ? beam.GetComponent<Renderer>() : null;
            laneRenderer = lane != null ? lane.GetComponent<Renderer>() : null;
            Hide();
        }

        /// <summary>Lays the beam from one point to another. <paramref name="strength"/> 0 to 1 for the streak; the lane's own for the ground.</summary>
        public void Show(Vector3 from, Vector3 to, float width, float strength, float laneStrength)
        {
            Vector3 along = to - from;
            along.y = 0f;
            float length = along.magnitude;
            if (length < 0.05f)
            {
                Hide();
                return;
            }
            var facing = Quaternion.LookRotation(along / length, Vector3.up);
            if (beam != null)
            {
                beam.gameObject.SetActive(strength > 0.01f);
                // The quad lies flat, so the camera above sees the whole streak.
                beam.SetPositionAndRotation((from + to) * 0.5f, facing * Quaternion.Euler(90f, 0f, 0f));
                beam.localScale = new Vector3(width * 0.6f, length, 1f);
                Paint(beamRenderer, BeamColour * strength);
            }
            if (lane != null)
            {
                lane.gameObject.SetActive(laneStrength > 0.01f);
                Vector3 middle = (from + to) * 0.5f;
                middle.y = Mathf.Min(from.y, to.y) - 1.15f;
                lane.SetPositionAndRotation(middle, facing * Quaternion.Euler(90f, 0f, 0f));
                lane.localScale = new Vector3(width, length, 1f);
                var colour = LaneColour * laneStrength;
                colour.a = LaneColour.a * laneStrength;
                Paint(laneRenderer, colour);
            }
        }

        public void Hide()
        {
            if (beam != null) beam.gameObject.SetActive(false);
            if (lane != null) lane.gameObject.SetActive(false);
        }

        private void Paint(Renderer target, Color colour)
        {
            if (target == null) return;
            block.Clear();
            block.SetColor(ColourId, colour);
            target.SetPropertyBlock(block);
        }
    }
}
