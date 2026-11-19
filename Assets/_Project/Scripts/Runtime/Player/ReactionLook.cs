using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// How a reaction is seen: the one place its look is made, so the real effects (the Blender look-dev, not yet
    /// approved) drop in here later. For now a placeholder: one puff in the two afflictions' colours together. The
    /// reaction's name over the enemy is the HUD's (<see cref="PlayerReactions.Shown"/>).
    /// </summary>
    public static class ReactionLook
    {
        public static void Play(int pair, Vector3 at)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return;
            Vfx.Play(VfxKind.TintPuff, at + Vector3.up * 1.3f, Vector3.up, 1.1f, Colour(pair));
        }

        /// <summary>The pair's two colours, half and half: the colour its name is written in.</summary>
        public static Color Colour(int pair) =>
            Color.Lerp(Afflictions.Spec(AfflictionPairs.First(pair)).Colour, Afflictions.Spec(AfflictionPairs.Second(pair)).Colour, 0.5f);
    }
}
