using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// Keeps the HUD's drawing board at least 1920 wide and at least 1080 tall whatever the screen's shape: a screen
    /// wider than 16:9 is fitted by its height and a narrower one by its width, so nothing laid out for 1920 by 1080
    /// runs off an edge at 4:3 or is blown up to fill a 32:9 strip.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class ScreenFit : MonoBehaviour
    {
        private const float Widescreen = 16f / 9f;

        private CanvasScaler scaler;
        private int fittedWidth, fittedHeight;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            Fit();
        }

        private void Update()
        {
            // The window can be resized, and Alt+Enter changes it, at any moment.
            if (Screen.width != fittedWidth || Screen.height != fittedHeight) Fit();
        }

        private void Fit()
        {
            fittedWidth = Screen.width;
            fittedHeight = Screen.height;
            if (fittedHeight <= 0) return;
            scaler.matchWidthOrHeight = fittedWidth / (float)fittedHeight >= Widescreen ? 1f : 0f;
        }

        /// <summary>Switches between the whole screen, at the display's own size, and a window.</summary>
        public static void ToggleFullscreen()
        {
            if (Screen.fullScreen)
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            }
            else
            {
                var display = Screen.currentResolution;
                Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
            }
        }
    }
}
