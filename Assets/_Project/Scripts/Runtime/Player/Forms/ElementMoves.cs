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
        // Gust and Stone Rise are elements 4's (the third review): fronts of curls parting round the enemies, and massive
        // stones bursting up under the one faced. Gust, Glare and Flung Blade are round 4's (the fourth review): curls only,
        // a blinding gold flash at each face struck, and a small forged blade flung end over end into the chest.
        private static readonly string[] Spells = { "flame_wave", null, "gust", null, "riptide", "briar_patch", "stone_rise", "flung_blade", "glare", null, "miasma" };
        private static readonly string[] Strikes = { null, null, "cutting_wind", "frost_bite", null, "grown_into_it", "broken_ground", null, null, "taken", null };
        // The strikes the Blender worker redrew from the owner's swing comments (2026-11-21), growing out of the cut: played
        // in place of the element's earlier strike once built (Clover > Build > Element Looks builds the approved ones).
        private static readonly string[] Redrawn = { null, null, null, null, null, null, "strike_earth", "strike_metal", "strike_light", "strike_dark", "strike_poison" };

        /// <summary>
        /// The reach the redrawn strikes' recipe was drawn for, metres: its carrier is the Katana's drawing cut (2.6 m), so a
        /// Katana plays it as drawn, a longer reach a little larger (the Massive Sword's, the Scythe's) and a shorter one a
        /// little smaller, never so far that the stones stand behind the enemies or swamp them.
        /// </summary>
        public const float RedrawnReach = 2.6f, SmallestFit = 0.85f, LargestFit = 1.3f;

        /// <summary>Tailwind, the wind's surge.</summary>
        public const string Tailwind = Prefix + "tailwind";

        // The keys, made once (nothing is made while a fight plays).
        private static readonly string[] HitKeys = Keys(Blades, "_hit1", "_hit2", "_hit3");
        private static readonly string[] SpellKeys = Keys(Spells, "", "@b");
        private static readonly string[] StrikeKeys = Keys(Strikes, "", "@b");
        private static readonly string[] RedrawnKeys = Keys(Redrawn, "", "@b");

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

        public static string StrikeKey(int element, bool onBody) =>
            IsRedrawn(element) ? Key(RedrawnKeys, 2, element, onBody ? 1 : 0) : Key(StrikeKeys, 2, element, onBody ? 1 : 0);

        /// <summary>True when the element's infused strike plays its redrawn look (the end effect grown out of the cut).</summary>
        public static bool IsRedrawn(int element)
        {
            string about = RedrawnKey(element, false);
            return about != null && (ElementLooks.Find(about) >= 0 || ElementLooks.Find(RedrawnKey(element, true)) >= 0);
        }

        /// <summary>The key a redrawn strike is built under, built or not; null for an element with none.</summary>
        public static string RedrawnKey(int element, bool onBody) => Key(RedrawnKeys, 2, element, onBody ? 1 : 0);

        /// <summary>True when the element's charged blade lands in its elements 3 look.</summary>
        public static bool HasBlade(int element) => HitKey(element, 1) != null && ElementLooks.Find(HitKey(element, 1)) >= 0;

        public static bool HasSpell(int element) => SpellKey(element, false) != null && ElementLooks.Find(SpellKey(element, false)) >= 0;

        /// <summary>True when the element's spell has a look on the bodies it reaches (Stone Rise's is all there, none about its caster).</summary>
        public static bool HasSpellOnBody(int element) => SpellKey(element, true) != null && ElementLooks.Find(SpellKey(element, true)) >= 0;

        /// <summary>The spell's look on a spot no body stands on (Stone Rise cast where nothing is reached), played there once.</summary>
        public static bool SpellAt(int element, Vector3 feet, Vector3 forward)
        {
            string key = SpellKey(element, true);
            return key != null && ElementLooks.PlayAt(ElementLooks.Find(key), feet, Yaw(forward));
        }

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

        /// <summary>
        /// The spell's look on a body held at its feet (<paramref name="feet"/>, the look's way <paramref name="forward"/>) for
        /// up to <paramref name="keep"/> seconds, moved with it by <see cref="ElementLooks.MoveHeld"/> and taken off by
        /// <see cref="ElementLooks.EndHeld"/>: Flung Blade's blade standing in the chest while it stands there. -1 for none.
        /// </summary>
        public static int HoldSpellOnBody(int element, Vector3 feet, Vector3 forward, float keep)
        {
            string key = SpellKey(element, true);
            return key != null ? ElementLooks.Held(ElementLooks.Find(key), feet, Yaw(forward), 1f, keep) : -1;
        }

        /// <summary>The spell reaches a body: its look on that body, once while it plays (a cloud's bites do not begin it again).</summary>
        public static void SpellReached(int element, Transform body, Vector3 forward) => OnBody(SpellKey(element, true), body, forward);

        /// <summary>
        /// An infused strike is dealt from <paramref name="feet"/> toward <paramref name="forward"/>: its look about the
        /// striker and its look on each body it struck (the first <paramref name="count"/> of <paramref name="struck"/>).
        /// False when there is none.
        /// </summary>
        public static bool PlayStrike(int element, Vector3 feet, Vector3 forward, Component[] struck, int count) => PlayStrike(element, feet, forward, struck, count, 0f);

        /// <summary>
        /// The same, a redrawn strike's arc fitted to the weapon's <paramref name="reach"/> (metres; 0 keeps the recipe's):
        /// its stones and glare follow the weapon's own tip, a Massive Sword's further out than a Katana's.
        /// </summary>
        public static bool PlayStrike(int element, Vector3 feet, Vector3 forward, Component[] struck, int count, float reach) =>
            PlayStrike(element, feet, forward, struck, count, reach, false);

        /// <summary>
        /// Degrees a blade must sweep round before its way is trusted: a thrust, a shot or a drop barely turns, and its
        /// strike is drawn as the recipe drew it.
        /// </summary>
        public const float SweepTrusted = 15f;

        /// <summary>
        /// True when a strike swept <paramref name="swept"/> degrees (seen from above, positive clockwise) must be drawn
        /// mirrored: the redrawn strikes' recipe cut right to left, anticlockwise, so a cut the other way has its glare,
        /// glint and stones mirrored about its own line to run with the blade.
        /// </summary>
        public static bool Mirrors(float swept) => swept > SweepTrusted;

        /// <summary>The same, mirrored about the strike's line when <paramref name="mirrored"/> (a redrawn strike only; the others keep their way).</summary>
        public static bool PlayStrike(int element, Vector3 feet, Vector3 forward, Component[] struck, int count, float reach, bool mirrored)
        {
            if (!ElementArt.Ready || !HasStrike(element)) return false;
            float size = reach > 0f && IsRedrawn(element) ? Mathf.Clamp(reach / RedrawnReach, SmallestFit, LargestFit) : 1f;
            // Either half may be empty: Frost Bite is all on the bodies it strikes, Broken Ground all on its line.
            bool mirror = mirrored && IsRedrawn(element);
            ElementLooks.PlayAt(ElementLooks.Find(StrikeKey(element, false)), feet, Yaw(forward), size, mirror);
            string onBody = StrikeKey(element, true);
            for (int i = 0; struck != null && i < count && i < struck.Length; i++)
                if (struck[i] != null) OnBody(onBody, struck[i].transform, forward, mirror);
            return true;
        }

        // The strike shapes the Blender worker drew for the weapons whose infused strike is not an arc (ElementLookBuilder.Slams):
        // each element's look about the shape's origin and on each body it reaches, by the element's place on the ring.
        /// <summary>The strike shapes, in order: the Great Hammer's slam, the Scythe's spin, the Great Spear's thrust, the Twin Knives' dash-through, the Chain Sickle's lash, the Great Bow's shot.</summary>
        public static readonly string[] Shapes = { "slam", "spin", "thrust", "dash", "lash", "shot" };
        public const int Slam = 0, Spin = 1, Thrust = 2, Dash = 3, Lash = 4, Shot = 5;
        private static readonly string[] RingWords = { "fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison" };
        private static readonly string[][] ShapeKeys = MakeShapeKeys();

        private static string[][] MakeShapeKeys()
        {
            var made = new string[Shapes.Length][];
            for (int s = 0; s < Shapes.Length; s++)
            {
                var slugs = new string[RingWords.Length];
                for (int e = 0; e < RingWords.Length; e++) slugs[e] = Shapes[s] + "_" + RingWords[e];
                made[s] = Keys(slugs, "", "@b");
            }
            return made;
        }

        /// <summary>The key a shape's look is built under, about its origin or on a body; null for none.</summary>
        public static string ShapeKey(int shape, int element, bool onBody) =>
            shape >= 0 && shape < Shapes.Length ? Key(ShapeKeys[shape], 2, element, onBody ? 1 : 0) : null;

        /// <summary>True when the element's look in that shape is built (the weapon's infused strike plays it in place of the arc's).</summary>
        public static bool HasShape(int shape, int element) => ShapeKey(shape, element, false) != null && ElementLooks.Find(ShapeKey(shape, element, false)) >= 0;

        /// <summary>Seconds before the blow that the look begins (Lightning's bolt on the hammer's raised head); 0 for none.</summary>
        public static float ShapeLead(int shape, int element) =>
            ElementLooks.TryLook(ElementLooks.Find(ShapeKey(shape, element, false)), out var look) ? look.Lead : 0f;

        /// <summary>The look about <paramref name="at"/> (the slam's impact, else where the player stands), turned to the blow's way. False when there is none.</summary>
        public static bool PlayShape(int shape, int element, Vector3 at, Vector3 forward) =>
            ElementArt.Ready && ElementLooks.PlayAt(ElementLooks.Find(ShapeKey(shape, element, false)), at, Yaw(forward));

        /// <summary>The look on each body the strike reached (the first <paramref name="count"/> of <paramref name="struck"/>).</summary>
        public static void ShapeStruck(int shape, int element, Component[] struck, int count, Vector3 forward)
        {
            if (!ElementArt.Ready) return;
            string key = ShapeKey(shape, element, true);
            for (int i = 0; struck != null && i < count && i < struck.Length; i++)
                if (struck[i] != null) OnBody(key, struck[i].transform, forward);
        }

        /// <summary>The shot's trace about the player as the arrow is loosed, turned along its flight; a handle to end it by (ElementLooks.EndHeld), -1 for none.</summary>
        public static int PlayShotTrace(int element, Vector3 at, Vector3 forward) =>
            ElementArt.Ready ? ElementLooks.Held(ElementLooks.Find(ShapeKey(Shot, element, false)), at, Yaw(forward), 1f, 0f) : -1;

        /// <summary>The key of the shot's impact look, about where the arrow stopped in someone (ElementLookBuilder.Shots); null for none.</summary>
        public static string ShotImpactKey(int element)
        {
            string about = ShapeKey(Shot, element, false);
            return about != null ? about + "@i" : null;
        }

        /// <summary>
        /// The shot's arrow has stopped in someone at <paramref name="at"/>, flying <paramref name="forward"/>: its impact
        /// there and its look on each one struck. False when the element has no shot built.
        /// </summary>
        public static bool ShotLands(int element, Vector3 at, Vector3 forward, Component[] struck, int count)
        {
            if (!ElementArt.Ready || !HasShape(Shot, element)) return false;
            int impact = ElementLooks.Find(ShotImpactKey(element));
            if (impact >= 0) ElementLooks.PlayAt(impact, at, Yaw(forward));
            ShapeStruck(Shot, element, struck, count, forward);
            return true;
        }

        /// <summary>The slam's (the Great Hammer's) keys and plays, as <see cref="ShapeKey"/> and its fellows.</summary>
        public static string SlamKey(int element, bool onBody) => ShapeKey(Slam, element, onBody);
        public static bool HasSlam(int element) => HasShape(Slam, element);
        public static float SlamLead(int element) => ShapeLead(Slam, element);
        public static bool PlaySlam(int element, Vector3 impact, Vector3 forward) => PlayShape(Slam, element, impact, forward);
        public static void SlamStruck(int element, Component[] struck, int count, Vector3 forward) => ShapeStruck(Slam, element, struck, count, forward);

        private static void OnBody(string key, Transform body, Vector3 forward, bool mirror = false)
        {
            if (key == null || body == null) return;
            int look = ElementLooks.Find(key);
            if (look < 0 || ElementLooks.IsPlaying(look, body)) return;
            ElementLooks.Play(look, body, Yaw(forward), mirror);
        }
    }
}
