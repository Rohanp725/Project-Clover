using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// Makes low health felt: a dark red edge round the screen that breathes slowly while the player is badly hurt, and
    /// harder when they are nearly gone. It only ever darkens the edges; the middle of the screen, where the fight is,
    /// stays clear. It runs on real time, so a stopped or slowed world does not hold its breath.
    /// </summary>
    public class LowHealthVignette : MonoBehaviour
    {
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private Image edge;
        [SerializeField] private CanvasGroup group;

        /// <summary>It shows under this share of health, and is at its strongest under the second.</summary>
        public float ShowBelow = 0.3f;
        public float WorstBelow = 0.15f;
        /// <summary>How strong it is when it first shows, against 1 at its worst.</summary>
        public float FirstStrength = 0.5f;
        public float BreathsPerSecond = 0.45f;
        public float WorstBreathsPerSecond = 0.8f;
        /// <summary>How far the breath lets it fall back, as a share of its strength.</summary>
        public float BreathDepth = 0.4f;
        public float FadePerSecond = 2.5f;

        /// <summary>How strongly it shows now, before the breath: 0 when the player is well.</summary>
        public float Strength { get; private set; }

        private float phase;

        public void Configure(PlayerDefence playerToWatch, Image edgeImage, CanvasGroup edgeGroup)
        {
            defence = playerToWatch;
            edge = edgeImage;
            group = edgeGroup;
        }

        private void Start()
        {
            if (edge != null) edge.enabled = false;
            if (group != null) group.alpha = 0f;
        }

        private void Update()
        {
            if (defence == null || edge == null || group == null) return;
            float health = defence.Health.Fraction;
            // The dead are past warning: the edge lets go and leaves the screen to what comes next.
            bool hurt = !defence.IsDead && health < ShowBelow;
            float worst = Mathf.InverseLerp(ShowBelow, WorstBelow, health);
            float wanted = hurt ? Mathf.Lerp(FirstStrength, 1f, worst) : 0f;
            Strength = Mathf.MoveTowards(Strength, wanted, FadePerSecond * Time.unscaledDeltaTime);
            if (Strength <= 0.001f)
            {
                if (edge.enabled) edge.enabled = false;
                return;
            }
            phase += Time.unscaledDeltaTime * Mathf.Lerp(BreathsPerSecond, WorstBreathsPerSecond, worst) * Mathf.PI * 2f;
            if (phase > Mathf.PI * 2f) phase -= Mathf.PI * 2f;
            float breath = 1f - BreathDepth * (0.5f - 0.5f * Mathf.Sin(phase));
            if (!edge.enabled) edge.enabled = true;
            // The alpha of the group is changed, not the colour of the image, so the canvas does not rebuild every frame.
            group.alpha = Strength * breath;
        }
    }
}
