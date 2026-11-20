using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// Bends time for a moment: nearly stops it for a few hundredths of a second when a blow lands, which is the cheapest
    /// way to make a hit feel heavy, and slows it a little longer to mark a perfect dodge. Time is always put back to
    /// what it was, including when the object is disabled, because domain reload is off and a stuck time scale would
    /// carry into the next play. A paused game (time scale zero) is left alone, and if anything else changes the time
    /// scale during a stop (a pause menu opening), the stop lets go and leaves that value in place. A menu that saved the
    /// stop's own nearly-frozen scale when it opened puts that back when it closes: the stop remembers what it wrote, and
    /// when time comes back to it once the stop's moment is over, it puts back the scale from before the stop (else the
    /// world was left at three per cent speed after resting straight after a blow, found by BranchProbe on 2026-11-20).
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        /// <summary>How fast time runs during a stop; not zero, so things still creep and the freeze reads as impact.</summary>
        public float FrozenTimeScale = 0.03f;

        private bool bent;
        private float freezeUntil;
        private float slowUntil;
        private float slowScale = 1f;
        private float scaleBefore = 1f;
        private float written = 1f;
        // A stop let go of mid-way (a menu took the time scale): the scale it had written, and the one from before it.
        private float orphaned = -1f, orphanedBefore = 1f;

        /// <summary>Nearly stops time for this many real seconds.</summary>
        public void Freeze(float seconds)
        {
            if (seconds <= 0f || !Begin()) return;
            freezeUntil = Mathf.Max(freezeUntil, Time.unscaledTime + seconds);
            Apply();
        }

        /// <summary>Runs time at a fraction of its speed for this many real seconds. A freeze inside it still bites.</summary>
        public void Slow(float seconds, float scale)
        {
            if (seconds <= 0f || !Begin()) return;
            scale = Mathf.Clamp01(scale);
            bool running = Time.unscaledTime < slowUntil;
            slowUntil = Mathf.Max(running ? slowUntil : 0f, Time.unscaledTime + seconds);
            slowScale = running ? Mathf.Min(slowScale, scale) : scale;
            Apply();
        }

        // A blow that lands while the game is paused must not start time again when the stop ends.
        private bool Begin()
        {
            if (bent) return true;
            if (Time.timeScale <= 0f) return false;
            // Begun while a menu's restored stop is still in force: the scale before is the one from before that stop.
            if (orphaned > 0f && Mathf.Approximately(Time.timeScale, orphaned)) Time.timeScale = orphanedBefore;
            scaleBefore = Time.timeScale;
            orphaned = -1f;
            freezeUntil = 0f;
            slowUntil = 0f;
            written = scaleBefore;
            bent = true;
            return true;
        }

        private void Update()
        {
            if (bent) Apply();
            else if (orphaned > 0f) Reclaim();
        }

        // Time has come back to the scale a stop wrote after the stop was let go: a menu restored it as it closed. The stop's
        // moment is over, so the scale from before it is put back. Any other scale (time set going again, a slow motion of
        // its own) means it is someone else's, and is left; a pause (nought) is waited out.
        private void Reclaim()
        {
            if (Time.timeScale <= 0f) return;
            if (Mathf.Approximately(Time.timeScale, orphaned)) Time.timeScale = orphanedBefore;
            orphaned = -1f;
        }

        /// <summary>The time scale with no stop in it: what time runs at once any stop now running is over.</summary>
        public float Unbent => bent ? scaleBefore : orphaned > 0f && Mathf.Approximately(Time.timeScale, orphaned) ? orphanedBefore : Time.timeScale;

        private void Apply()
        {
            // Someone else has set the time scale since this last wrote it: it is theirs now.
            if (!Mathf.Approximately(Time.timeScale, written))
            {
                bent = false;
                if (!Mathf.Approximately(written, scaleBefore))
                {
                    orphaned = written;
                    orphanedBefore = scaleBefore;
                }
                return;
            }
            float now = Time.unscaledTime;
            if (now < freezeUntil) Write(FrozenTimeScale * scaleBefore);
            else if (now < slowUntil) Write(slowScale * scaleBefore);
            else Release();
        }

        private void OnDisable()
        {
            Release();
            if (orphaned > 0f && Mathf.Approximately(Time.timeScale, orphaned)) Time.timeScale = orphanedBefore;
            orphaned = -1f;
        }

        private void Release()
        {
            if (!bent) return;
            bent = false;
            if (Mathf.Approximately(Time.timeScale, written)) Time.timeScale = scaleBefore;
        }

        private void Write(float scale)
        {
            written = scale;
            Time.timeScale = scale;
        }
    }
}
