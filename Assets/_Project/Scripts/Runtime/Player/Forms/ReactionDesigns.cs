using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Which of the 55 reactions have a designed look and how the rest are drawn until they do. The 24 designed ones
    /// (combos pass 2) are built from the look-dev's recipe into <see cref="ReactionArtSet"/>; every other pair is
    /// composed from its two parents' element art (<see cref="ElementArtSet"/>): each parent's own piece on the body and
    /// one ground mark, the second parent's if it has one. Also the rule that turns the look-dev's lamps into the game's.
    /// Plain tables, so tests can read them without a scene.
    /// </summary>
    public static class ReactionDesigns
    {
        /// <summary>The elements on the ring as the kits name them, in the order of <see cref="AfflictionPairs.Ring"/>.</summary>
        public static readonly string[] Ring = { "fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison" };

        /// <summary>The recipe's 24 combos by their pair key (combos_recipe.json, notes.md "Combos pass 2").</summary>
        public static readonly string[] Designed =
        {
            "fire_ice", "fire_lightning", "poison_fire", "lightning_ice", "light_dark", "dark_lightning", "wind_water", "wind_ice",
            "water_light", "earth_fire", "poison_water", "dark_poison", "poison_lightning", "water_earth", "wind_earth", "earth_metal",
            "life_metal", "life_dark", "fire_water", "metal_light", "fire_life", "lightning_earth", "dark_ice", "light_lightning",
        };

        /// <summary>
        /// The looks with a way to them: drawn with the player on the side the look-dev's player stood (Rime's comb on the
        /// approach and its feathers behind, Capsize's wind from the windward side, Logan Stone's comb from the side,
        /// Stolen Warmth's breath running to the player). The rest are turned to the camera, as their frames were drawn.
        /// </summary>
        public static readonly string[] Directed = { "wind_ice", "wind_water", "wind_earth", "dark_ice" };

        /// <summary>The most lamps one look lights at once; dimmer ones in the recipe are left out.</summary>
        public const int MostLampsPerLook = 2;

        /// <summary>A lamp reaches no farther than this, however strong, and no less than the least.</summary>
        public const float LeastLampRange = 1.5f, MostLampRange = 5f;

        /// <summary>The pair a recipe key names ("fire_ice", in either order); -1 when it names none.</summary>
        public static int PairOf(string key)
        {
            if (string.IsNullOrEmpty(key)) return -1;
            int bar = key.IndexOf('_');
            if (bar <= 0 || bar >= key.Length - 1) return -1;
            int first = RingIndex(key.Substring(0, bar)), second = RingIndex(key.Substring(bar + 1));
            if (first < 0 || second < 0) return -1;
            return AfflictionPairs.Index(AfflictionPairs.Ring[first], AfflictionPairs.Ring[second]);
        }

        /// <summary>An element's place on the ring by its kit name; -1 for none.</summary>
        public static int RingIndex(string element)
        {
            for (int i = 0; i < Ring.Length; i++)
                if (Ring[i] == element) return i;
            return -1;
        }

        /// <summary>An affliction's place on the ring; -1 for one not on it.</summary>
        public static int RingIndex(AfflictionKind kind)
        {
            for (int i = 0; i < AfflictionPairs.Ring.Length; i++)
                if (AfflictionPairs.Ring[i] == kind) return i;
            return -1;
        }

        /// <summary>True when the pair is one of the 24 the look-dev designed.</summary>
        public static bool IsDesigned(int pair)
        {
            for (int i = 0; i < Designed.Length; i++)
                if (PairOf(Designed[i]) == pair) return true;
            return false;
        }

        public static bool IsDirected(string key) => System.Array.IndexOf(Directed, key) >= 0;

        // ---- The composed looks: each element's own piece on a body, and its ground mark if it has one.

        // Pieces that sit at a body's feet in the kit: Fire's globs bursting into tongues, Lightning's arcs crawling on the
        // limbs, Wind's paper slips and leaves, Ice's frost tiers, Water's after-splash, Life's sprout, Earth's slab jut and
        // cubes, Metal's broken clang arcs, Light's rayed ring (stood behind the body), Dark's streaks drawn in, Poison's
        // bubbling wound.
        private static readonly string[] bodyPieces =
        {
            "fire_globs_hit", "lightning_crawl_affliction", "wind_debris_hit", "ice_tiers_affliction", "water_aftersplash_hit",
            "life_sprout_affliction", "earth_cubes_hit", "metal_clang_hit", "light_prabhavali_hit", "dark_streaksair_hit", "poison_wound_hit",
        };

        // The ground an element leaves under a body, where its kit has one: soot, filings, the lotus, the drained disc, the
        // stain. Lightning, Wind, Ice, Water, Life and Earth leave none of their own at a hit's size.
        private static readonly string[] marks =
        {
            "fire_sootring_hit", null, null, null, null, null, null, "metal_filings_affliction", "light_lotus_hit", "dark_drain_hit", "poison_stain_hit",
        };

        private static readonly float[] markScales = { 0.8f, 0f, 0f, 0f, 0f, 0f, 0f, 0.9f, 0.65f, 0.8f, 0.8f };

        /// <summary>The kit piece an element shows on a body in a composed reaction, by its place on the ring.</summary>
        public static string BodyPiece(int element) => element >= 0 && element < bodyPieces.Length ? bodyPieces[element] : null;

        /// <summary>The ground mark an element leaves in a composed reaction; null when it leaves none.</summary>
        public static string Mark(int element) => element >= 0 && element < marks.Length ? marks[element] : null;

        public static float MarkScale(int element) => element >= 0 && element < markScales.Length ? markScales[element] : 1f;

        /// <summary>
        /// The pieces a composed reaction draws: the first and second parents' body pieces, and the ground mark (the
        /// second parent's, else the first's, else none). Every name is a piece of the element art.
        /// </summary>
        public static void Composed(int pair, out string first, out string second, out string mark, out int markElement)
        {
            int a = RingIndex(AfflictionPairs.First(pair)), b = RingIndex(AfflictionPairs.Second(pair));
            first = BodyPiece(a);
            second = BodyPiece(b);
            markElement = Mark(b) != null ? b : Mark(a) != null ? a : -1;
            mark = Mark(markElement);
        }

        // ---- The lamps.

        /// <summary>
        /// A Blender point lamp's power in watts as a URP point light's intensity. Blender's lamp of P watts lights a
        /// surface d metres off with P / (4 pi^2 d^2) (its watt carries an extra pi against the radiometric one), and URP's
        /// of intensity I with I / d^2, so I = P / (4 pi^2): 395 W is 10. The game's toon floors step the light into
        /// bands, so the intensity sets how far each band reaches more than how bright it is.
        /// </summary>
        public static float LampIntensity(float watts) => Mathf.Max(0f, watts) / (4f * Mathf.PI * Mathf.PI);

        /// <summary>
        /// How far the lamp reaches: the recipe's own cut-off when it has one (above zero), else where its light falls to
        /// half of a plain light's, sqrt(2 I); held between 1.5 and 5 m so a reaction lights its own ground, not the room.
        /// </summary>
        public static float LampRange(float watts, float recipeRange)
        {
            float range = recipeRange > 0f ? recipeRange : Mathf.Sqrt(2f * LampIntensity(watts));
            return Mathf.Clamp(range, LeastLampRange, MostLampRange);
        }

        /// <summary>
        /// Which lamps a look keeps: the brightest, at most <paramref name="most"/>, in the recipe's order. Writes their
        /// indices into <paramref name="kept"/> and returns how many.
        /// </summary>
        public static int KeepBrightest(float[] watts, int most, int[] kept)
        {
            int count = 0;
            if (watts == null || kept == null) return 0;
            most = Mathf.Min(most, kept.Length);
            for (int i = 0; i < watts.Length; i++)
            {
                // Brighter than how many, and as bright as how many earlier: kept if fewer than `most` beat it.
                int beaten = 0;
                for (int j = 0; j < watts.Length; j++)
                    if (watts[j] > watts[i] || (watts[j] == watts[i] && j < i)) beaten++;
                if (beaten < most && count < most) kept[count++] = i;
            }
            return count;
        }
    }
}
