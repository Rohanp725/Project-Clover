using UnityEngine;

namespace Clover.UI
{
    /// <summary>
    /// Lets something on the HUD stand for a while as a scene opens and then fade and go: a quiet line that is worth
    /// saying once and not worth keeping on the screen.
    /// </summary>
    public class FadeAway : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;

        /// <summary>Real seconds it stands before it starts to fade, and real seconds the fade takes.</summary>
        public float HoldSeconds = 8f;
        public float FadeSeconds = 1f;

        public bool Finished { get; private set; }

        private float elapsed;

        public void Configure(CanvasGroup groupToFade, float holdSeconds, float fadeSeconds)
        {
            group = groupToFade;
            HoldSeconds = holdSeconds;
            FadeSeconds = fadeSeconds;
        }

        private void Update()
        {
            if (Finished || group == null) return;
            // On real time, so it also goes while a menu has the world stopped. One long frame as the scene loads must
            // not eat the seconds it was meant to be read in.
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            float fading = elapsed - HoldSeconds;
            if (fading < 0f) return;
            if (FadeSeconds > 0f && fading < FadeSeconds)
            {
                group.alpha = 1f - fading / FadeSeconds;
                return;
            }
            Finished = true;
            group.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
