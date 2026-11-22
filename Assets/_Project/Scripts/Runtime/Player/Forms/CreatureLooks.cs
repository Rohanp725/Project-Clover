using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The effect-only spell creatures (owner question 3: effects shaped like the creature, no model), drawn in Blender as
    /// recipe looks in staging/blender/2026-11-23_creatures/creatures_recipe.json and played here by their slugs once the
    /// editor's look builder has taken that recipe in. Until then <see cref="Play"/> finds nothing and the spell's own form
    /// draws it as before.
    /// </summary>
    public static class CreatureLooks
    {
        /// <summary>Devouring Maw's jaw of smoke rising out of your shadow and snapping shut on the Cursed.</summary>
        public const string Jaw = "cr_jaw";
        /// <summary>The Black Buffalo's horned bulk of smoke charging down its lane.</summary>
        public const string Horns = "cr_horns";
        /// <summary>Blight's Swarm crawling from the broken jar to the next enemy.</summary>
        public const string Swarm = "cr_swarm";
        /// <summary>Second Shadow's copy standing up behind you.</summary>
        public const string Copy = "cr_copy";
        /// <summary>Nothing Stays Buried's shade standing up out of the fallen.</summary>
        public const string Shades = "cr_shades";
        /// <summary>Sapling's young tree springing up at the attacker's feet and lashing.</summary>
        public const string Sapling = "cr_sapling";

        /// <summary>Every creature slug, in the recipe's order.</summary>
        public static readonly string[] All = { Jaw, Horns, Swarm, Copy, Shades, Sapling };

        /// <summary>
        /// Plays a creature's look once: on <paramref name="body"/> when it names one, else standing at <paramref name="at"/>,
        /// turned so the recipe's player faces along <paramref name="facing"/>. False when the look is not in the game yet.
        /// </summary>
        public static bool Play(string slug, Transform body, Vector3 at, Vector3 facing)
        {
            int look = ElementLooks.Find(slug);
            if (look < 0) return false;
            float yaw = YawOf(facing);
            return body != null ? ElementLooks.Play(look, body, yaw) : ElementLooks.PlayAt(look, at, yaw);
        }

        /// <summary>The heading in degrees of a flat way (0 along +Z, 90 along +X).</summary>
        public static float YawOf(Vector3 facing)
        {
            facing.y = 0f;
            return facing.sqrMagnitude > 0.0001f ? Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg : 0f;
        }
    }
}
