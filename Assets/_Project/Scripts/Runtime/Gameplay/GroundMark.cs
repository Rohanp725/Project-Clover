using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// One mark an attack draws on the ground before it lands: a still outline of the shape and a brighter copy that
    /// grows to fill it as the blow comes near. The fighter that owns it says where, how far along, and in what colour.
    /// </summary>
    public class GroundMark
    {
        private static readonly int MarkerColour = Shader.PropertyToID("_Color");

        private readonly Renderer outline;
        private readonly Renderer fill;
        private readonly Transform outlineAt;
        private readonly Transform fillAt;
        private readonly MaterialPropertyBlock block;

        public bool Shown { get; private set; }

        public GroundMark(MeshFilter outlineFilter, MeshFilter fillFilter, Mesh shape, MaterialPropertyBlock shared)
        {
            block = shared;
            if (outlineFilter == null || fillFilter == null) return;
            outlineFilter.sharedMesh = shape;
            fillFilter.sharedMesh = shape;
            outlineAt = outlineFilter.transform;
            fillAt = fillFilter.transform;
            outline = outlineFilter.GetComponent<Renderer>();
            fill = fillFilter.GetComponent<Renderer>();
            Hide();
        }

        /// <summary>
        /// Draws the mark. A round one fills from its middle and a lane along its length. <paramref name="size"/>
        /// grows or shrinks the whole shape, <paramref name="lengthScale"/> the length of a lane alone, and
        /// <paramref name="strength"/> fades it.
        /// </summary>
        public void Show(Vector3 at, Quaternion facing, float progress, Color colour, bool lane, float size = 1f, float lengthScale = 1f, float strength = 1f)
        {
            if (outline == null) return;
            Shown = true;
            outlineAt.SetPositionAndRotation(at + Vector3.up * 0.05f, facing);
            fillAt.SetPositionAndRotation(at + Vector3.up * 0.07f, facing);
            float shown = Mathf.Clamp01(progress);
            outlineAt.localScale = new Vector3(size, 1f, size * lengthScale);
            fillAt.localScale = lane ? new Vector3(size, 1f, size * lengthScale * shown) : new Vector3(size * shown, 1f, size * shown);
            Tint(outline, colour, MarkerShapes.OutlineAlpha * strength);
            if (shown > 0.001f) Tint(fill, MarkerShapes.Growing(colour), MarkerShapes.GrowingAlpha * strength);
            else fill.enabled = false;
        }

        public void Hide()
        {
            Shown = false;
            if (outline != null) outline.enabled = false;
            if (fill != null) fill.enabled = false;
        }

        private void Tint(Renderer shown, Color colour, float alpha)
        {
            shown.enabled = true;
            colour.a = alpha;
            block.Clear();
            block.SetColor(MarkerColour, colour);
            shown.SetPropertyBlock(block);
        }
    }
}
