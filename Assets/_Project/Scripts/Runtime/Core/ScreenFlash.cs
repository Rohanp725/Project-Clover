using UnityEngine;
using UnityEngine.UI;

namespace Clover.Core
{
    /// <summary>
    /// The impact frame: for a few hundredths of a second spikes of one flat colour are driven in from the screen's
    /// edges, as a comic panel does at the moment of a blow; the middle stays clear so the blow itself is seen. It sits on the HUD as a full-screen image; anything may call
    /// <see cref="Flash"/>. With none in the scene the call does nothing.
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        [SerializeField] private Image wash;

        private static ScreenFlash instance;

        private float left;
        private float seconds;
        private Color colour;

        // Domain reload is off: the flash of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public void Configure(Image washImage) => wash = washImage;

        private void OnEnable()
        {
            instance = this;
            if (wash != null) wash.enabled = false;
        }

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Washes the screen with a colour (its alpha is how strongly) that is gone after the given real seconds.</summary>
        public static void Flash(Color washColour, float realSeconds)
        {
            if (instance == null || instance.wash == null || realSeconds <= 0f) return;
            // A stronger flash already showing is not replaced by a weaker one.
            if (instance.left > 0f && instance.colour.a * (instance.left / instance.seconds) > washColour.a) return;
            instance.colour = washColour;
            instance.seconds = realSeconds;
            instance.left = realSeconds;
            instance.wash.color = washColour;
            instance.wash.enabled = true;
        }

        private void Update()
        {
            if (left <= 0f) return;
            // Real time: the frame is seen through the stop that comes with the blow.
            left -= Time.unscaledDeltaTime;
            if (left <= 0f)
            {
                wash.enabled = false;
                return;
            }
            // Held flat for the first half, then gone quickly: a frame, not a fade.
            var shown = colour;
            shown.a *= Mathf.Clamp01(left / seconds * 2f);
            wash.color = shown;
        }
    }
}
