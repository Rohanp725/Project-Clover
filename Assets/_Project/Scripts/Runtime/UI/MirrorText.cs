using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// Shows one label's words through another, with something behind them, and shows or hides both as the first label
    /// is switched on or off. It lets a line that other code only knows as a plain label be drawn over a backing.
    /// </summary>
    public class MirrorText : MonoBehaviour
    {
        [SerializeField] private Text source;
        [SerializeField] private Text shown;
        [SerializeField] private GameObject backing;

        private string last;
        // Begins as if shown, so the first look puts the backing away when there are no words yet.
        private bool wasOn = true;

        public void Configure(Text sourceText, Text shownText, GameObject behind)
        {
            source = sourceText;
            shown = shownText;
            backing = behind;
        }

        private void LateUpdate()
        {
            if (source == null || shown == null) return;
            bool on = source.enabled && !string.IsNullOrEmpty(source.text);
            if (on != wasOn)
            {
                wasOn = on;
                shown.enabled = on;
                if (backing != null) backing.SetActive(on);
            }
            // The words are copied only when they change.
            if (on && !ReferenceEquals(source.text, last))
            {
                last = source.text;
                shown.text = last;
            }
        }
    }
}
