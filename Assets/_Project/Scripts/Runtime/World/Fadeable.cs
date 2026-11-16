using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A tall piece of the world that thins out to a see-through screen of dots when it stands between the camera and
    /// the player, and fills in again when it no longer does. It still casts its shadow. The camera's
    /// <see cref="OcclusionFader"/> tells it when; this only shows the result.
    /// </summary>
    public class Fadeable : MonoBehaviour
    {
        private static readonly int FadeId = Shader.PropertyToID("_Fade");

        private Renderer shown;
        private MaterialPropertyBlock block;
        private float fade = 1f;

        /// <summary>How solid the piece is now, from 0 to 1.</summary>
        public float Fade => fade;

        private void Awake()
        {
            shown = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
        }

        /// <summary>Moves toward the given solidity. Returns true once it has arrived.</summary>
        public bool Step(float target, float amount)
        {
            if (shown == null) return true;
            fade = Mathf.MoveTowards(fade, target, amount);
            if (fade >= 0.999f)
            {
                shown.SetPropertyBlock(null);
            }
            else
            {
                block.SetFloat(FadeId, fade);
                shown.SetPropertyBlock(block);
            }
            return Mathf.Approximately(fade, target);
        }
    }
}
