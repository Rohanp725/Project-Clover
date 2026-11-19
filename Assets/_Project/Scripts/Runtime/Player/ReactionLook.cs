using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// How a reaction is seen: the one place its look is made. With the element art on, the 24 reactions the look-dev
    /// designed show their approved look (<see cref="Forms.ReactionArt"/>) and the other 31 a look composed from their two
    /// parents' element art; with it off (F8) or absent, the old placeholder: one puff in the two afflictions' colours.
    /// The reaction's name over the enemy is the HUD's (<see cref="PlayerReactions.Shown"/>).
    /// </summary>
    public static class ReactionLook
    {
        /// <summary>The pair's look at a body whose feet are at <paramref name="at"/>, not followed, turned to the camera.</summary>
        public static void Play(int pair, Vector3 at) => Play(pair, at, null, Vector3.zero);

        /// <summary>
        /// The pair's look at a body whose feet are at <paramref name="at"/>. Its body parts ride on
        /// <paramref name="body"/> while it stands; <paramref name="away"/> is the way the blow carried (from the player
        /// through the enemy), which lays a look with a way to it (Rime, Capsize, Logan Stone, Stolen Warmth) along it.
        /// </summary>
        public static void Play(int pair, Vector3 at, Transform body, Vector3 away)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return;
            if (Forms.ElementArt.Ready)
            {
                if (!Forms.ReactionArt.Play(pair, at, body, away)) Forms.ReactionArt.Compose(pair, at, away);
                return;
            }
            Vfx.Play(VfxKind.TintPuff, at + Vector3.up * 1.3f, Vector3.up, 1.1f, Colour(pair));
        }

        /// <summary>The pair's two colours, half and half: the colour its name is written in.</summary>
        public static Color Colour(int pair) =>
            Color.Lerp(Afflictions.Spec(AfflictionPairs.First(pair)).Colour, Afflictions.Spec(AfflictionPairs.Second(pair)).Colour, 0.5f);
    }
}
