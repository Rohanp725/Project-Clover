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

        // Pieces drawn over this one (a modelled house front on a box of mud brick) that thin out with it. Empty unless
        // a builder fills it.
        [SerializeField] private Renderer[] alsoShown = new Renderer[0];
        // The merged scenery's copies of what lies on or against this piece, made afresh each time the scene opens, so
        // never saved: kept apart from the saved list, which still holds what is drawn by itself (a swaying line of linen).
        [System.NonSerialized] private Renderer[] mergedShown = new Renderer[0];

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

        /// <summary>Other renderers that thin out and fill in with this piece.</summary>
        public void AlsoFade(Renderer[] others) => alsoShown = others ?? new Renderer[0];

        /// <summary>The renderers saved as thinning out with this piece.</summary>
        public Renderer[] AlsoShown => alsoShown;

        /// <summary>The merged scenery's renderers that thin out with this piece (set each time the scene opens).</summary>
        public void AlsoFadeMerged(Renderer[] merged) => mergedShown = merged ?? new Renderer[0];

        /// <summary>Moves toward the given solidity. Returns true once it has arrived.</summary>
        public bool Step(float target, float amount)
        {
            if (shown == null) return true;
            fade = Mathf.MoveTowards(fade, target, amount);
            bool whole = fade >= 0.999f;
            if (!whole) block.SetFloat(FadeId, fade);
            shown.SetPropertyBlock(whole ? null : block);
            Apply(alsoShown, whole);
            Apply(mergedShown, whole);
            return Mathf.Approximately(fade, target);
        }

        private void Apply(Renderer[] others, bool whole)
        {
            for (int i = 0; i < others.Length; i++)
                if (others[i] != null) others[i].SetPropertyBlock(whole ? null : block);
        }
    }
}
