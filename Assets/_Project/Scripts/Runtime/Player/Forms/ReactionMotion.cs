using System;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How a part of a reaction's look moves over its time (docs/GAME.md, "Reaction effects in the game"): the verbs
    /// (a bolt falls, a wave rises, wind sweeps in, a mark spreads, flame licks, steam drifts, a ring pulses...) as plain
    /// numbers for a given age, so <see cref="ReactionArt"/> can pose its pooled renderers with them and the tests can
    /// check them without a scene. Nothing here allocates.
    ///
    /// A part's <see cref="Cue"/> says which verb, in which beat of the look (the cause arriving, the impact, the
    /// aftermath), when it starts, how long its own motion takes and how long it lasts. <see cref="Evaluate"/> turns a
    /// look's age into a <see cref="Pose"/>: how far along its path it still has to go, how it is stretched along and
    /// across its axis, how big it is, how high it has drifted, how far it has turned, whether it is shown this frame
    /// (a strobe), how see-through, how white (the flash where it lands) and how much of a mark is drawn on. The caller
    /// gives the path, the axis and the pivot; this gives the amounts.
    /// </summary>
    public static class ReactionMotion
    {
        /// <summary>The verbs. Pop is the look as first built: it pops in, holds and goes.</summary>
        public const byte Pop = 0, Fall = 1, Rise = 2, Sink = 3, Sweep = 4, Extend = 5, Spread = 6, Spin = 7, Flicker = 8, Drift = 9, Lick = 10, Pulse = 11, Burst = 12, Blow = 13;

        public static readonly string[] VerbNames = { "pop", "fall", "rise", "sink", "sweep", "extend", "spread", "spin", "flicker", "drift", "lick", "pulse", "burst", "blow" };

        /// <summary>The three beats of a look: the cause arrives, the impact, the aftermath (the first to go under load).</summary>
        public const byte Arrive = 0, Impact = 1, Aftermath = 2;

        public static readonly string[] BeatNames = { "arrive", "impact", "aftermath" };

        /// <summary>What <see cref="Evaluate"/> says of a part at an age: not started yet, showing, or over.</summary>
        public const int Waiting = 0, Showing = 1, Over = 2;

        /// <summary>How long the white frame lasts where a cold hard thing (a bolt, ice, steel, a ray) lands.</summary>
        public const float FlashSeconds = 0.06f;

        /// <summary>The share of a part's time at its end in which it shrinks (solids) or fades (glows, marks) away.</summary>
        public const float OutShare = 0.35f;

        /// <summary>When and how one part moves.</summary>
        [Serializable]
        public struct Cue
        {
            public byte Verb;
            public byte Beat;
            /// <summary>Seconds after the look starts that this part appears.</summary>
            public float Delay;
            /// <summary>Seconds its own motion takes (the fall, the rise, the sweep, the strobe); it holds after.</summary>
            public float Move;
            /// <summary>Seconds from its start until it is gone.</summary>
            public float Seconds;
            /// <summary>
            /// The verb's size: metres fallen, swept, blown or drifted; degrees spun; beats pulsed; a spread's first share;
            /// metres a rise springs up from under the ground (none: it grows where it stands); degrees a flicker is
            /// turned at each redraw (none: it is redrawn where it was).
            /// </summary>
            public float Amount;
            /// <summary>How many times it is redrawn on and off over its motion (lightning); none for a steady part.</summary>
            public byte Strobes;
            /// <summary>A white frame where it lands.</summary>
            public bool Flash;

            /// <summary>Seconds after the look starts that it is gone.</summary>
            public float End => Delay + Seconds;

            /// <summary>True for a cue that was set (a look built before the motion has none).</summary>
            public bool Set => Seconds > 0f;

            /// <summary>Seconds after the look starts that it lands: the end of a fall, a sweep or a ray, else its start.</summary>
            public float LandsAt => Delay + (Lands(Verb) ? Move : 0f);

            public static Cue Of(byte verb, byte beat, float delay, float move, float seconds, float amount = 0f, byte strobes = 0, bool flash = false) =>
                new Cue { Verb = verb, Beat = beat, Delay = delay, Move = move, Seconds = seconds, Amount = amount, Strobes = strobes, Flash = flash };

            public override string ToString() =>
                $"{VerbNames[Verb < VerbNames.Length ? Verb : 0]} {BeatNames[Beat < BeatNames.Length ? Beat : 0]} at {Delay:0.00} s for {Seconds:0.00} s" +
                (Strobes > 0 ? $", {Strobes} strobes" : "") + (Flash ? ", flash" : "");
        }

        /// <summary>Where a part is in its motion: amounts the caller turns into a transform and a property block.</summary>
        public struct Pose
        {
            /// <summary>The share of its path still to go: 1 at the path's start, 0 in its place.</summary>
            public float Travel;
            /// <summary>Its stretch along its axis (up for most; the fall, the sweep or the ray's way for those).</summary>
            public float Along;
            /// <summary>Its stretch across its axis.</summary>
            public float Across;
            /// <summary>Its size, the same every way.</summary>
            public float Size;
            /// <summary>Metres it has drifted up.</summary>
            public float Lift;
            /// <summary>Degrees it has turned about the up axis.</summary>
            public float Spin;
            /// <summary>Whole (1) to gone (0) over its last share of time: a solid shrinks by it, a glow and a mark fade by it.</summary>
            public float Left;
            /// <summary>How white it is drawn: 1 on the frame it lands, when it flashes.</summary>
            public float Hot;
            /// <summary>How much of a mark is drawn on (0 none, 1 all).</summary>
            public float Reveal;
            /// <summary>False on a strobe's dark frames.</summary>
            public bool Shown;
            /// <summary>Which redraw of a strobe this is: each is drawn a little off the last.</summary>
            public int Redraw;
        }

        /// <summary>True for the verbs whose impact comes at the end of their motion (they travel in and land).</summary>
        public static bool Lands(byte verb) => verb == Fall || verb == Sweep || verb == Extend || verb == Rise || verb == Sink || verb == Lick || verb == Spread;

        /// <summary>
        /// The pose of a part with this cue when its look is <paramref name="age"/> seconds old. <paramref name="seed"/>
        /// keeps two licking or strobing parts out of step. Returns <see cref="Waiting"/>, <see cref="Showing"/> or <see cref="Over"/>.
        /// </summary>
        public static int Evaluate(in Cue cue, float age, int seed, out Pose pose)
        {
            pose = new Pose { Along = 1f, Across = 1f, Size = 1f, Left = 1f, Reveal = 1f, Shown = true };
            float local = age - cue.Delay;
            if (local < 0f) return Waiting;
            if (local >= cue.Seconds) return Over;
            float move = Mathf.Max(0.01f, cue.Move);
            float p = Mathf.Clamp01(local / move);
            float after = local - move;
            float rest = Mathf.Max(0.01f, cue.Seconds - move);
            pose.Left = Left(local, cue.Seconds);

            switch (cue.Verb)
            {
                case Fall:
                    // Accelerating down its slant, drawn long with its speed; squashed a moment where it lands.
                    pose.Travel = 1f - p * p;
                    if (p < 1f)
                    {
                        pose.Along = 1.45f;
                        pose.Across = 0.8f;
                    }
                    else
                    {
                        float q = Mathf.Clamp01(after / 0.12f);
                        pose.Along = Mathf.Lerp(0.72f, 1f, EaseOut(q));
                        pose.Across = Mathf.Lerp(1.15f, 1f, EaseOut(q));
                    }
                    break;
                case Rise:
                    if (cue.Amount > 0f)
                    {
                        // Shoved up whole out of the ground from below, past its place and back (the owner's anvil
                        // "springs up from the ground"): the floor hides what is still under it.
                        pose.Lift = cue.Amount * (Overshoot(p, 0.22f) - 1f);
                        pose.Along = Mathf.Lerp(0.85f, 1f, EaseOut(p));
                        break;
                    }
                    // Up out of the ground past its height and back: it lands as a shape, not as a growth.
                    pose.Along = Overshoot(p, 0.25f);
                    pose.Across = Mathf.Lerp(0.7f, 1f, EaseOut(p));
                    break;
                case Sink:
                    // Held up and spread wide, it drops into place, gathering inward as it goes, faster at the end.
                    pose.Lift = cue.Amount * (1f - p * p);
                    // A third wider at the start, so the slabs are seen gathering in round the pit (15% read as still).
                    pose.Across = Mathf.Lerp(1.35f, 1f, p * p);
                    pose.Size = Mathf.Lerp(0.6f, 1f, EaseOut(Mathf.Clamp01(p * 3f)));
                    break;
                case Sweep:
                {
                    // In from the far end of its path, fast then slowing, stretched along its way while it moves.
                    float e = 1f - (1f - p) * (1f - p);
                    pose.Travel = 1f - e;
                    pose.Along = 1f + 0.9f * (1f - p);
                    pose.Across = Mathf.Lerp(0.7f, 1f, p);
                    pose.Size = Mathf.Lerp(0.6f, 1f, e);
                    break;
                }
                case Extend:
                    // Drawn out from its root to its far end.
                    pose.Along = Mathf.Max(0.02f, EaseOut(p));
                    pose.Across = Mathf.Lerp(0.6f, 1f, p);
                    break;
                case Spread:
                {
                    // Out from the middle, then still creeping a little.
                    float start = cue.Amount > 0f ? Mathf.Clamp01(cue.Amount) : 0.15f;
                    float e = EaseOut(p);
                    pose.Across = Mathf.Lerp(start, 1f, e) * (1f + 0.05f * Mathf.Clamp01(after / rest));
                    pose.Reveal = e;
                    break;
                }
                case Spin:
                    // Turning all its life, slowing; drawn in from wider as it starts (a spiral in).
                    pose.Spin = cue.Amount * EaseOut(Mathf.Clamp01(local / Mathf.Max(0.01f, cue.Seconds)));
                    pose.Size = Mathf.Lerp(1.4f, 1f, EaseOut(p));
                    break;
                case Flicker:
                    // Strobes through its motion, then holds with a short dark blink now and then.
                    if (p >= 1f) pose.Shown = Mathf.Repeat(after * 7f + seed * 0.37f, 1f) < 0.82f;
                    break;
                case Drift:
                {
                    // Up and out, thinning as it goes.
                    float e = Mathf.Clamp01(local / Mathf.Max(0.01f, cue.Seconds));
                    pose.Lift = cue.Amount * (1f - (1f - e) * (1f - e));
                    pose.Size = Mathf.Lerp(0.7f, 1.2f, e) * Grow(p);
                    pose.Left = Mathf.Min(pose.Left, 1f - Mathf.Clamp01((e - 0.45f) / 0.55f));
                    break;
                }
                case Lick:
                {
                    // Up from its root, then never still: taller and thinner, shorter and wider, out of step with the next.
                    float wave = (local * 5.5f + seed * 0.27f) * Mathf.PI * 2f;
                    float flutter = 0.16f * Mathf.Sin(wave) + 0.08f * Mathf.Sin(2.3f * wave + seed);
                    pose.Along = Overshoot(p, 0.18f) * (1f + flutter * p);
                    pose.Across = Mathf.Lerp(0.75f, 1f, p) * (1f - 0.45f * flutter);
                    break;
                }
                case Pulse:
                {
                    // Out with a bang, then beating, each beat smaller.
                    pose.Size = Grow(p);
                    if (p >= 1f)
                    {
                        float t = Mathf.Clamp01(after / rest);
                        pose.Size *= 1f + 0.14f * Mathf.Abs(Mathf.Sin(Mathf.PI * Mathf.Max(1f, cue.Amount) * t)) * (1f - t);
                    }
                    break;
                }
                case Burst:
                {
                    // Out from the middle as it grows (the pieces fly from the body), then still going a little.
                    pose.Size = Mathf.Lerp(0.08f, 1f, EaseOut(p)) * (1f + 0.12f * Mathf.Clamp01(after / rest));
                    break;
                }
                case Blow:
                {
                    // Out from the body as a burst, then carried on down the wind all its life, drawn long while it
                    // flies: the blow's way is the caller's (Rime's ice blown out of the far side).
                    pose.Size = Mathf.Lerp(0.3f, 1f, EaseOut(p));
                    pose.Travel = EaseOut(Mathf.Clamp01(local / Mathf.Max(0.01f, cue.Seconds)));
                    pose.Along = 1f + 0.35f * (1f - pose.Travel);
                    break;
                }
                default:
                    pose.Size = Grow(p);
                    break;
            }

            // A strobe over any verb: on, off and on again through its motion, each redraw a little off the last.
            if (cue.Strobes > 0 && p < 1f)
            {
                pose.Shown = Strobe(local, move, cue.Strobes, out pose.Redraw);
            }
            else if (cue.Verb == Flicker && p < 1f)
            {
                pose.Shown = Strobe(local, move, 2, out pose.Redraw);
            }
            // Static that crawls: each redraw turned on from the last, so it is never seen held in one shape.
            if (cue.Verb == Flicker && cue.Amount != 0f) pose.Spin = pose.Redraw * cue.Amount;

            if (cue.Flash)
            {
                float landsAt = Lands(cue.Verb) ? move : 0f;
                pose.Hot = local >= landsAt && local < landsAt + FlashSeconds ? 1f : 0f;
            }
            return Showing;
        }

        /// <summary>
        /// Lit or dark at <paramref name="local"/> seconds into a motion of <paramref name="move"/> seconds redrawn
        /// <paramref name="strobes"/> times: each redraw is lit for its first three fifths. <paramref name="redraw"/> is which.
        /// </summary>
        public static bool Strobe(float local, float move, int strobes, out int redraw)
        {
            int flashes = Mathf.Max(1, strobes) + 1;
            float period = Mathf.Max(0.01f, move) / flashes;
            float at = Mathf.Max(0f, local) / period;
            redraw = Mathf.Min(flashes - 1, Mathf.FloorToInt(at));
            return at - Mathf.Floor(at) < 0.6f || local >= move;
        }

        /// <summary>Whole until the last <see cref="OutShare"/> of its time, then down to nothing.</summary>
        public static float Left(float local, float seconds)
        {
            float outFrom = seconds * (1f - OutShare);
            if (local <= outFrom) return 1f;
            float t = Mathf.Clamp01((local - outFrom) / Mathf.Max(0.01f, seconds - outFrom));
            return 1f - t * t;
        }

        /// <summary>Out fast and a little past its size, then back.</summary>
        public static float Grow(float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Max(0.02f, 1f - (1f - t) * (1f - t) * (1f - t) + 0.12f * Mathf.Sin(t * Mathf.PI));
        }

        /// <summary>From nothing to past its height by <paramref name="over"/> and back to 1.</summary>
        public static float Overshoot(float t, float over)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Max(0.02f, 1f - (1f - t) * (1f - t) * (1f - t) + over * Mathf.Sin(t * Mathf.PI));
        }

        public static float EaseOut(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }
    }
}
