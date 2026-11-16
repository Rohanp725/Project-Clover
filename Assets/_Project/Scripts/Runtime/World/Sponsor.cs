using Clover.Core;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The sponsor, in the hub: a light that hangs in the air and speaks. The first time the player wakes here it
    /// speaks unasked; after that it waits to be spoken to. Its words are in <see cref="SponsorLines"/>.
    /// </summary>
    public class Sponsor : Interactable
    {
        [SerializeField] private Transform body;
        [SerializeField] private Light glow;

        /// <summary>Seconds after the scene opens before it speaks unasked, the first time.</summary>
        public float SpeaksAfter = 1.4f;
        public float BobHeight = 0.18f;
        public float BobSeconds = 3.2f;
        public float GlowWhenSpeaking = 1.6f;

        public int TimesSpoken { get; private set; }

        private Vector3 restPosition;
        private float restGlow;
        private float untilFirstWords = -1f;
        private bool spokenThisVisit;

        public void Configure(Transform bodyToMove, Light glowLight)
        {
            body = bodyToMove;
            glow = glowLight;
        }

        private void Start()
        {
            if (body != null) restPosition = body.localPosition;
            if (glow != null) restGlow = glow.intensity;
            if (!Progress.Has(Progress.MetSponsor)) untilFirstWords = SpeaksAfter;
        }

        public override void Use(PlayerInteraction who) => Speak();

        private void Speak()
        {
            TimesSpoken++;
            bool met = Progress.Has(Progress.MetSponsor);
            string[] lines = spokenThisVisit ? SponsorLines.NothingMore : SponsorLines.For(met, RunState.RunsEnded, RunState.LastRunFell, RunState.LastRunGods);
            spokenThisVisit = true;
            Progress.Unlock(Progress.MetSponsor);
            ReadingBox.Show(SponsorLines.Name, lines);
        }

        private void Update()
        {
            if (untilFirstWords > 0f)
            {
                untilFirstWords -= Time.deltaTime;
                if (untilFirstWords <= 0f)
                {
                    // Not over someone else's words: it waits until the box is free.
                    if (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen) untilFirstWords = 0.2f;
                    else Speak();
                }
            }
            // It is never quite still, and it brightens while it speaks.
            if (body != null)
                body.localPosition = restPosition + Vector3.up * (Mathf.Sin(Time.time * Mathf.PI * 2f / BobSeconds) * BobHeight);
            if (glow != null)
            {
                bool speaking = ReadingBox.Instance != null && ReadingBox.Instance.IsOpen;
                glow.intensity = Mathf.MoveTowards(glow.intensity, restGlow * (speaking ? GlowWhenSpeaking : 1f), Time.deltaTime * restGlow * 2f);
            }
        }
    }
}
