using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Each element's own moments as the Blender worker drew them in elements 3 (staging/blender/2026-11-20_elements3),
    /// built by Clover > Build > Element Looks into the looks the pool plays (<see cref="ElementLooks"/>): the charged
    /// blade's blows landing ("e3_fire_blade_hit1" to "hit3", the third with the element's extra), the spells and the
    /// infused strikes (one look about the player, "e3_flame_wave", and one on each body it reaches, "e3_flame_wave@b"),
    /// and Tailwind. The blade's own sheath is laid on the real weapon by the swing trails. Where a look is built the old
    /// pieces of that moment are not drawn beside it (the owner: no look goes back without the change asked for).
    /// </summary>
    public static class ElementMoves
    {
        public const string Prefix = "e3_";

        // By the element's place on the ring: Fire, Lightning, Wind, Ice, Water, Life, Earth, Metal, Light, Dark, Poison.
        // Lightning's spell and blade were called good (2026-11-19) and keep their looks.
        private static readonly string[] Blades =
            { "fire_blade", null, "wind_blade", "ice_blade", "water_blade", "life_blade", "earth_blade", "metal_blade", "light_blade", "dark_blade", "poison_blade" };
        private static readonly string[] Spells = { "flame_wave", null, null, null, "riptide", "briar_patch", null, null, null, null, "miasma" };
        private static readonly string[] Strikes = { null, null, "cutting_wind", "frost_bite", null, "grown_into_it", "broken_ground", null, null, "taken", null };

        /// <summary>Tailwind, the wind's surge.</summary>
        public const string Tailwind = Prefix + "tailwind";

        // The keys, made once (nothing is made while a fight plays).
        private static readonly string[] HitKeys = Keys(Blades, "_hit1", "_hit2", "_hit3");
        private static readonly string[] SpellKeys = Keys(Spells, "", "@b");
        private static readonly string[] StrikeKeys = Keys(Strikes, "", "@b");

        private static string[] Keys(string[] slugs, params string[] endings)
        {
            var keys = new string[slugs.Length * endings.Length];
            for (int e = 0; e < slugs.Length; e++)
                for (int k = 0; k < endings.Length; k++)
                    keys[e * endings.Length + k] = slugs[e] != null ? Prefix + slugs[e] + endings[k] : null;
            return keys;
        }

        private static string Key(string[] keys, int perElement, int element, int which) =>
            element >= 0 && element * perElement + which < keys.Length ? keys[element * perElement + which] : null;

        /// <summary>The recipe's slug of an element's charged blade ("fire_blade"); null for none.</summary>
        public static string BladeSlug(int element) => element >= 0 && element < Blades.Length ? Blades[element] : null;

        /// <summary>The look of the charged blade's <paramref name="blow"/>th blow (1 to 3) landing; null for none.</summary>
        public static string HitKey(int element, int blow) => Key(HitKeys, 3, element, Mathf.Clamp(blow, 1, 3) - 1);

        public static string SpellKey(int element, bool onBody) => Key(SpellKeys, 2, element, onBody ? 1 : 0);

        public static string StrikeKey(int element, bool onBody) => Key(StrikeKeys, 2, element, onBody ? 1 : 0);

        /// <summary>True when the element's charged blade lands in its elements 3 look.</summary>
        public static bool HasBlade(int element) => HitKey(element, 1) != null && ElementLooks.Find(HitKey(element, 1)) >= 0;

        public static bool HasSpell(int element) => SpellKey(element, false) != null && ElementLooks.Find(SpellKey(element, false)) >= 0;

        public static bool HasStrike(int element) =>
            StrikeKey(element, false) != null && (ElementLooks.Find(StrikeKey(element, false)) >= 0 || ElementLooks.Find(StrikeKey(element, true)) >= 0);

        private static float Yaw(Vector3 forward)
        {
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg : 0f;
        }

        /// <summary>A charged blow lands on <paramref name="body"/>, swung <paramref name="forward"/>: its blow's look on it. False when there is none.</summary>
        public static bool PlayBladeHit(int element, Transform body, Vector3 forward, int blow)
        {
            string key = HitKey(element, blow);
            if (key == null || body == null) return false;
            return ElementLooks.Play(ElementLooks.Find(key), body, Yaw(forward));
        }

        /// <summary>
        /// A spell is cast: its look about <paramref name="anchor"/> (where the recipe's caster stood, the look's own way
        /// <paramref name="forward"/>), kept at least <paramref name="keep"/> seconds while what it laid lasts. A handle to
        /// move it by (<see cref="ElementLooks.MoveHeld"/>), or -1 when there is none.
        /// </summary>
        public static int PlaySpell(int element, Vector3 anchor, Vector3 forward, float size = 1f, float keep = 0f)
        {
            string key = SpellKey(element, false);
            return key != null ? ElementLooks.Held(ElementLooks.Find(key), anchor, Yaw(forward), size, keep) : -1;
        }

        /// <summary>Metres ahead of its caster the recipe's spell is about (the recipe's origin): an area spell's middle.</summary>
        public const float SpellAhead = 3f;

        /// <summary>The spell reaches a body: its look on that body, once while it plays (a cloud's bites do not begin it again).</summary>
        public static void SpellReached(int element, Transform body, Vector3 forward) => OnBody(SpellKey(element, true), body, forward);

        /// <summary>
        /// An infused strike is dealt from <paramref name="feet"/> toward <paramref name="forward"/>: its look about the
        /// striker and its look on each body it struck (the first <paramref name="count"/> of <paramref name="struck"/>).
        /// False when there is none.
        /// </summary>
        public static bool PlayStrike(int element, Vector3 feet, Vector3 forward, Component[] struck, int count)
        {
            if (!ElementArt.Ready || !HasStrike(element)) return false;
            // Either half may be empty: Frost Bite is all on the bodies it strikes, Broken Ground all on its line.
            ElementLooks.PlayAt(ElementLooks.Find(StrikeKey(element, false)), feet, Yaw(forward));
            string onBody = StrikeKey(element, true);
            for (int i = 0; struck != null && i < count && i < struck.Length; i++)
                if (struck[i] != null) OnBody(onBody, struck[i].transform, forward);
            return true;
        }

        private static void OnBody(string key, Transform body, Vector3 forward)
        {
            if (key == null || body == null) return;
            int look = ElementLooks.Find(key);
            if (look < 0 || ElementLooks.IsPlaying(look, body)) return;
            ElementLooks.Play(look, body, Yaw(forward));
        }
    }
}
