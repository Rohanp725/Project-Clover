using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The name of a place, or of whoever waits in it, shown large for a few seconds as the player arrives: a stroke of
    /// ink is drawn across the screen, the name appears on it, and both fade. It asks nothing of the player and tells
    /// them nothing they need.
    /// </summary>
    public class TitleCard : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform stroke;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;

        /// <summary>Real seconds: before it begins, drawing in, held, and fading.</summary>
        public float Delay = 0.7f;
        public float DrawSeconds = 0.45f;
        public float HoldSeconds = 2.2f;
        public float FadeSeconds = 0.9f;

        public bool Finished { get; private set; }

        private float elapsed;
        private float strokeWidth;

        public void Configure(CanvasGroup groupToFade, RectTransform strokeToDraw, Text titleText, Text subtitleText)
        {
            group = groupToFade;
            stroke = strokeToDraw;
            title = titleText;
            subtitle = subtitleText;
        }

        private void Start()
        {
            if (stroke != null) strokeWidth = stroke.sizeDelta.x;
            Show(0f, 0f);
        }

        private void Update()
        {
            if (Finished || group == null) return;
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed - Delay;
            if (t < 0f) return;
            if (t < DrawSeconds)
            {
                float drawn = t / DrawSeconds;
                Show(1f - (1f - drawn) * (1f - drawn), drawn);
            }
            else if (t < DrawSeconds + HoldSeconds) Show(1f, 1f);
            else if (t < DrawSeconds + HoldSeconds + FadeSeconds) Show(1f, 1f - (t - DrawSeconds - HoldSeconds) / FadeSeconds);
            else
            {
                Finished = true;
                group.gameObject.SetActive(false);
            }
        }

        // The stroke is drawn from its middle outward; the words come with it.
        private void Show(float drawn, float alpha)
        {
            group.alpha = alpha;
            if (stroke != null) stroke.sizeDelta = new Vector2(strokeWidth * (0.3f + 0.7f * drawn), stroke.sizeDelta.y);
        }
    }
}
