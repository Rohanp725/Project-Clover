using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// Nearly stops time for a few hundredths of a second when a blow lands. It is the cheapest way to make a hit feel
    /// heavy. Time is always put back to what it was, including when the object is disabled, because domain reload is off
    /// and a stuck time scale would carry into the next play. A paused game (time scale zero) is left alone.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        /// <summary>How fast time runs during the stop; not zero, so things still creep and the freeze reads as impact.</summary>
        public float FrozenTimeScale = 0.03f;

        private bool frozen;
        private float releaseAt;
        private float scaleBefore = 1f;

        public void Freeze(float seconds)
        {
            // A blow that lands while the game is paused must not start time again when the stop ends.
            if (seconds <= 0f || (!frozen && Time.timeScale <= 0f)) return;
            if (!frozen) scaleBefore = Time.timeScale;
            releaseAt = Mathf.Max(frozen ? releaseAt : 0f, Time.unscaledTime + seconds);
            frozen = true;
            Time.timeScale = FrozenTimeScale * scaleBefore;
        }

        private void Update()
        {
            if (frozen && Time.unscaledTime >= releaseAt) Release();
        }

        private void OnDisable() => Release();

        private void Release()
        {
            if (!frozen) return;
            frozen = false;
            Time.timeScale = scaleBefore;
        }
    }
}
