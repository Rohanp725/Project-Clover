using Clover.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The Fire land's two lines on the HUD: the sun mark, which fills through the warning of a swell and burns red-gold
    /// while the swell burns the player, and the count of embers waiting to be reclaimed from the enemies carrying them.
    /// Each is shown only while it has something to say.
    /// </summary>
    public class LandHud : MonoBehaviour
    {
        [SerializeField] private Image sunRing;
        [SerializeField] private Image sunFill;
        [SerializeField] private Text reclaim;

        public Color Filling = new Color(1f, 0.93f, 0.7f, 1f);
        public Color Swelling = new Color(1f, 0.78f, 0.3f, 1f);
        public Color Burning = new Color(1f, 0.35f, 0.12f, 1f);

        private int shownReclaim = -1;
        private bool sunShown = true;

        public void Configure(Image ring, Image fill, Text reclaimText)
        {
            sunRing = ring;
            sunFill = fill;
            reclaim = reclaimText;
        }

        private void Awake()
        {
            ShowSun(HeatSchedule.Phase.Calm, 0f, false);
            ShowReclaim(0);
        }

        /// <summary>Where the swell stands, as the sun mark shows it.</summary>
        public void ShowSun(HeatSchedule.Phase phase, float progress, bool burning)
        {
            bool shown = phase != HeatSchedule.Phase.Calm;
            if (shown != sunShown)
            {
                sunShown = shown;
                if (sunRing != null) sunRing.enabled = shown;
                if (sunFill != null) sunFill.enabled = shown;
            }
            if (!shown || sunFill == null) return;
            if (phase == HeatSchedule.Phase.Warning)
            {
                sunFill.fillAmount = progress;
                sunFill.color = Filling;
            }
            else
            {
                // Full through the swell, emptying as it ends; pulsing hot while it is burning the player.
                sunFill.fillAmount = 1f - progress;
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 9f);
                sunFill.color = burning ? Color.Lerp(Swelling, Burning, pulse) : Swelling;
            }
        }

        /// <summary>How many embers wait in the enemies carrying them; nothing is shown at none.</summary>
        public void ShowReclaim(int embers)
        {
            if (embers == shownReclaim || reclaim == null) return;
            shownReclaim = embers;
            reclaim.enabled = embers > 0;
            if (embers > 0) reclaim.text = $"Shards to reclaim: {embers}";
        }
    }
}
