using Clover.Gameplay;
using UnityEngine;
using Cue = Clover.Player.Forms.ReactionMotion.Cue;
using M = Clover.Player.Forms.ReactionMotion;

namespace Clover.Player.Forms
{
    /// <summary>
    /// When each part of a reaction's look moves, and how (docs/GAME.md, "Reaction effects in the game"). Every look
    /// plays as three beats over about 0.6 to 1.4 s: the cause arrives, the impact, the aftermath.
    ///
    /// A recipe piece's motion role comes from its kit object's name (<see cref="RoleOf"/>: a bolt strikes, tongues
    /// lick, steam drifts, chips burst, a splash rises, streaks sweep in, a clang rings, mites swirl, flat pieces are
    /// marks that spread). The builder groups a look's pieces by place, element and role, and each group takes its cue
    /// from the look's timeline here: a step naming a word of the piece's name first, then a step naming its role,
    /// else the role's default. The 24 timelines are written from the look-dev's notes ("Combos pass 2") and what each
    /// reaction does (docs/design/playtest/effects.md). The 31 composed looks follow one rule by the parents' nature
    /// (<see cref="Composed"/>). Plain tables, so tests can read them without a scene.
    /// </summary>
    public static class ReactionTimelines
    {
        /// <summary>Motion roles: what a piece is, by its name, for how it moves.</summary>
        public const byte Still = 0, Strike = 1, Ray = 2, Static = 3, Flame = 4, Vapour = 5, Debris = 6, Wave = 7, Growth = 8, Wind = 9, Ring = 10, Swirl = 11, Mark = 12;

        public static readonly string[] RoleNames = { "still", "strike", "ray", "static", "flame", "vapour", "debris", "wave", "growth", "wind", "ring", "swirl", "mark" };

        // The words of a kit object's name that give its role, tried in this order (the first found wins): the
        // look-dev's names say what a piece is (notes.md, "Combos pass 2").
        private static readonly byte[] Order = { Swirl, Strike, Ray, Static, Flame, Vapour, Debris, Wave, Wind, Ring, Growth };

        private static readonly string[][] Words =
        {
            new string[0],
            new[] { "truebolt", "blackbolt", "bolt_", "strike", "pencil" },
            new[] { "beam", "breath", "warmthring", "fuse" },
            new[] { "crackle", "crawl", "lightning_arc", "lightning_flash", "nerveruyi" },
            new[] { "tongue", "climb" },
            new[] { "steam", "smoke", "fog", "gas", "ruyi", "ember", "flakes", "bubbles", "poison_lip" },
            new[] { "chip", "shard", "spark", "pellet", "drop", "dust", "life_leaf", "cubes" },
            new[] { "splash", "crown", "curtain", "wet" },
            new[] { "thorn", "vine", "briar", "root", "sprout", "tiers", "cluster", "rime", "flower", "stone", "slab", "heave", "anvil", "jut" },
            new[] { "streak", "comb", "curl", "crescent", "grass", "wind_leaf", "pinned" },
            new[] { "clang", "star", "glint", "bead", "ring", "eclipse", "burst", "plate", "notch", "bandlip", "seams", "ichor", "bubble", "corona", "crackweb" },
            new[] { "streaksair", "mite", "coils", "ashknot" },
            new string[0],
        };

        private static readonly string[] Prefixes = { "vfx3c_", "vfx2c_", "vfx3_", "vfx2_", "vfx_", "v2_", "c2_", "combo_" };

        /// <summary>A kit object's name without the kit's prefixes, lower case: "vfx3c_vfx_combo_mite" is "mite".</summary>
        public static string Plain(string kitObject)
        {
            string name = (kitObject ?? "").ToLowerInvariant();
            bool cut = true;
            while (cut)
            {
                cut = false;
                foreach (string prefix in Prefixes)
                    if (name.StartsWith(prefix))
                    {
                        name = name.Substring(prefix.Length);
                        cut = true;
                    }
            }
            return name;
        }

        /// <summary>The motion role of a piece standing up (not a flat mark) by its kit object's name; <see cref="Still"/> when no word fits.</summary>
        public static byte RoleOf(string kitObject)
        {
            string name = Plain(kitObject);
            foreach (byte role in Order)
                foreach (string word in Words[role])
                    if (name.Contains(word)) return role;
            return Still;
        }

        /// <summary>A role by its name; 255 for none.</summary>
        public static byte RoleNamed(string name)
        {
            for (int i = 0; i < RoleNames.Length; i++)
                if (RoleNames[i] == name) return (byte)i;
            return 255;
        }

        /// <summary>The elements whose pieces land with a white frame: the cold hard stuff. Flame, stone, leaf, smoke and water never flash white.</summary>
        public static bool Flashes(string element) => element == "lightning" || element == "ice" || element == "metal" || element == "light";

        // ---- Defaults by role, used when a look's timeline names neither the piece nor its role.

        private static readonly Cue[] Defaults =
        {
            Cue.Of(M.Pop, M.Impact, 0.2f, 0.1f, 0.9f),
            Cue.Of(M.Fall, M.Arrive, 0f, 0.2f, 0.65f, 5f, 2, true),
            Cue.Of(M.Extend, M.Impact, 0.15f, 0.25f, 0.8f, 0f, 0, true),
            Cue.Of(M.Flicker, M.Arrive, 0f, 0.3f, 0.6f, 0f, 3, true),
            Cue.Of(M.Lick, M.Impact, 0.2f, 0.18f, 0.9f),
            Cue.Of(M.Drift, M.Aftermath, 0.35f, 0.15f, 1f, 0.9f),
            Cue.Of(M.Burst, M.Impact, 0.22f, 0.22f, 0.75f, 0f, 0, true),
            Cue.Of(M.Rise, M.Impact, 0.2f, 0.22f, 0.8f),
            Cue.Of(M.Rise, M.Impact, 0.15f, 0.25f, 0.95f),
            Cue.Of(M.Sweep, M.Arrive, 0f, 0.3f, 0.75f, 2.5f),
            Cue.Of(M.Pulse, M.Impact, 0.25f, 0.08f, 0.8f, 3f, 0, true),
            Cue.Of(M.Spin, M.Arrive, 0f, 0.35f, 0.85f, 220f),
            Cue.Of(M.Spread, M.Aftermath, 0.3f, 0.35f, 1.4f, 0.4f),
        };

        public static Cue Default(byte role) => role < Defaults.Length ? Defaults[role] : Defaults[Still];

        // ---- The 24 designed looks.

        /// <summary>One line of a look's timeline: which pieces (a role's name, or a word of the kit object's name), when and how.</summary>
        public struct Step
        {
            public string Match;
            public Cue Cue;
            /// <summary>Seconds between the pieces it splits into, one kit object to a part (tongues climbing in threes); 0 keeps them as one.</summary>
            public float Stagger;
        }

        public struct Timeline
        {
            /// <summary>The recipe's pair key, "fire_lightning".</summary>
            public string Key;
            /// <summary>What the player sees, in order.</summary>
            public string Story;
            public Step[] Steps;
        }

        private static Step S(string match, byte verb, byte beat, float delay, float move, float seconds, float amount = 0f, byte strobes = 0, bool flash = false, float stagger = 0f) =>
            new Step { Match = match, Cue = Cue.Of(verb, beat, delay, move, seconds, amount, strobes, flash), Stagger = stagger };

        private static Timeline T(string key, string story, params Step[] steps) => new Timeline { Key = key, Story = story, Steps = steps };

        private const byte A = M.Arrive, I = M.Impact, F = M.Aftermath;

        /// <summary>How much slower the looks the owner found too fast to judge are played (Shut in the Dark, Fire Medicine).</summary>
        public const float SlowPace = 1.8f;

        /// <summary>How much slower than the others a look is played: <see cref="SlowPace"/> for the two slowed, else 1.</summary>
        public static float Pace(string key) => key == "poison_fire" || key == "dark_poison" ? SlowPace : 1f;

        // A timeline played slower: every step's start, motion and life drawn out by the factor, its staggers too.
        private static Timeline Slower(Timeline timeline, float factor)
        {
            for (int i = 0; i < timeline.Steps.Length; i++)
            {
                ref var step = ref timeline.Steps[i];
                step.Cue.Delay *= factor;
                step.Cue.Move *= factor;
                step.Cue.Seconds *= factor;
                step.Stagger *= factor;
            }
            return timeline;
        }

        public static readonly Timeline[] Designed =
        {
            T("fire_ice", "frost tiers crust the legs; the coal crack-web flares and ice chips and shards burst; steam and embers drift off as the char spreads",
                S("growth", M.Rise, A, 0f, 0.25f, 0.95f),
                S("ring", M.Pulse, I, 0.25f, 0.08f, 0.8f, 2f),
                S("static", M.Flicker, I, 0.25f, 0.25f, 0.5f, 0f, 2),
                S("debris", M.Burst, I, 0.28f, 0.22f, 0.7f, 0f, 0, true),
                S("vapour", M.Drift, F, 0.4f, 0.15f, 1f, 1f),
                S("mark", M.Spread, F, 0.3f, 0.35f, 1.1f, 0.45f)),
            // The owner's review (2026-11-19): the lightning comes down from the sky, strobing, and the orange climbing
            // flame is gone (the builder leaves it out), so the violet tongues are the fire.
            // 2026-11-20: all its flame is violet, embers too, up at the strike and gone within about a second.
            T("fire_lightning", "a bolt falls out of the sky, strobing; violet static crawls the limbs; the violet tongues climb in threes; violet embers and smoke rise as the char spreads",
                S("strike", M.Fall, A, 0f, 0.18f, 0.6f, 6f, 3, true),
                S("static", M.Flicker, I, 0.18f, 0.3f, 0.55f, 0f, 4, true),
                S("flame", M.Lick, I, 0.24f, 0.18f, 0.85f, 0f, 0, false, 0.1f),
                S("vapour", M.Drift, F, 0.5f, 0.15f, 0.9f, 1f),
                S("mark", M.Spread, F, 0.3f, 0.35f, 1.1f, 0.4f)),
            // Slowed by Slower: the owner could not judge it at its first pace (2026-11-19).
            Slower(T("poison_fire", "the fumes hang and the fuse and pits show; the bang: the lobed ring pulses, flames lick and sparks burst; steam and embers rise",
                S("vapour", M.Drift, A, 0f, 0.15f, 1.2f, 0.6f),
                S("mark", M.Spread, A, 0f, 0.45f, 1.4f, 0.3f),
                S("ring", M.Pulse, I, 0.3f, 0.08f, 0.8f, 2f),
                S("flame", M.Lick, I, 0.32f, 0.15f, 0.85f, 0f, 0, false, 0.06f),
                S("debris", M.Burst, I, 0.3f, 0.2f, 0.7f)), SlowPace),
            T("lightning_ice", "frost crusts the body and static crackles over it; a bolt falls; pellets and chips burst; the glass fern spreads",
                S("growth", M.Rise, A, 0f, 0.25f, 1f),
                S("static", M.Flicker, A, 0.1f, 0.3f, 0.6f, 0f, 3, true),
                S("strike", M.Fall, I, 0.38f, 0.15f, 0.55f, 5f, 2, true),
                // The crack-web flares and the scorch is burnt where the bolt lands (0.53 s), not before it.
                S("ring", M.Pulse, I, 0.52f, 0.08f, 0.8f, 2f, 0, true),
                S("scorch", M.Pop, I, 0.53f, 0.1f, 0.85f),
                S("debris", M.Burst, I, 0.5f, 0.22f, 0.7f, 0f, 0, true),
                S("mark", M.Spread, F, 0.5f, 0.3f, 0.9f, 0.35f)),
            T("light_dark", "dark's streaks wheel in; the slatted shaft lands; the eclipse ring and its bead flare; the corona and shadow bands spread",
                S("swirl", M.Spin, A, 0f, 0.3f, 0.6f, -200f),
                S("strike", M.Fall, I, 0.2f, 0.18f, 0.7f, 4.5f, 0, true),
                S("ring", M.Pulse, I, 0.38f, 0.08f, 0.9f, 2f, 0, true),
                // The wheel and the shadow bands on the ground show where the shaft lands (0.38 s).
                S("still", M.Pop, I, 0.38f, 0.1f, 0.9f),
                S("mark", M.Spread, F, 0.38f, 0.4f, 1f, 0.3f)),
            // The owner's review (2026-11-19): nothing of the lightning is seen held still before the strike, only the
            // cyan crawling in the cloud (redrawn and turned every few frames); the cloud churns slowly, so the bolt's top
            // stays in it, and the black bolt is drawn down out of the cloud to the head, joined to both. The violet static
            // on the body comes with the strike.
            T("dark_lightning", "the ash knot gathers over the head with pale streaks drawn in; cyan crawls in it; the black bolt is drawn down out of it to the head; static runs on the body; the black fern spreads",
                S("swirl", M.Spin, A, 0f, 0.4f, 1.05f, 50f),
                S("knotcrawl", M.Flicker, A, 0.12f, 0.42f, 0.45f, 137f, 9, true),
                S("crawl", M.Flicker, I, 0.62f, 0.3f, 0.5f, 0f, 3, true),
                S("strike", M.Extend, I, 0.5f, 0.12f, 0.55f, 0f, 2, true),
                S("mark", M.Spread, F, 0.6f, 0.3f, 0.85f, 0.4f)),
            T("wind_water", "the grass flattens in from the windward side; the after-splash rises and drops fly; the foam combs out downwind",
                S("wind", M.Sweep, A, 0f, 0.3f, 0.7f, 2.5f),
                S("wave", M.Rise, I, 0.28f, 0.22f, 0.7f),
                S("debris", M.Burst, I, 0.32f, 0.22f, 0.6f),
                S("mark", M.Spread, F, 0.3f, 0.4f, 1f, 0.35f)),
            // The owner's review (2026-11-19): the crystals are seen blown on down the way the blow went, and the fog
            // with them (the builder lays the fan along that way too).
            T("wind_ice", "wind streaks sweep in; rime grows on the near side; chips blow out of the far side and on down the wind; fog is carried after them and the feathers fan out",
                S("wind", M.Sweep, A, 0f, 0.3f, 0.7f, 2.5f),
                S("growth", M.Rise, I, 0.22f, 0.25f, 1f),
                S("debris", M.Blow, I, 0.35f, 0.2f, 0.95f, 2.6f, 0, true),
                S("vapour", M.Blow, F, 0.4f, 0.2f, 1f, 1.4f),
                S("mark", M.Spread, F, 0.35f, 0.4f, 1f, 0.3f)),
            T("water_light", "the shaft falls on the first body; the crown splashes up and glints flash on the water; the net spreads across the pool",
                S("strike", M.Fall, A, 0f, 0.2f, 0.7f, 5f, 0, true),
                S("wave", M.Rise, I, 0.2f, 0.22f, 0.75f),
                S("debris", M.Burst, I, 0.22f, 0.22f, 0.6f),
                S("ring", M.Pulse, I, 0.3f, 0.08f, 0.8f, 3f, 0, true),
                S("mark", M.Spread, F, 0.2f, 0.5f, 1.1f, 0.2f)),
            T("earth_fire", "the heave ring rises and chips fly; three stones fall back hot; sparks and flames flare on them; smoke and flakes rise as the char spreads",
                S("stone", M.Fall, I, 0.15f, 0.3f, 0.95f, 3f),
                S("spark", M.Burst, I, 0.45f, 0.2f, 0.55f, 0f, 0, true),
                S("growth", M.Rise, A, 0f, 0.25f, 1f),
                S("debris", M.Burst, A, 0.05f, 0.25f, 0.6f),
                S("ring", M.Pulse, I, 0.45f, 0.08f, 0.6f, 2f),
                S("flame", M.Lick, I, 0.45f, 0.15f, 0.65f),
                S("vapour", M.Drift, F, 0.55f, 0.15f, 0.8f, 0.8f),
                S("mark", M.Spread, F, 0.45f, 0.3f, 0.9f, 0.4f)),
            T("poison_water", "fouled water runs off the body and the slick spreads; bubbles burst in rings; fumes rise",
                S("wave", M.Rise, A, 0f, 0.25f, 0.8f),
                S("mark", M.Spread, A, 0.05f, 0.6f, 1.4f, 0.3f),
                S("ring", M.Pulse, I, 0.3f, 0.08f, 0.9f, 3f),
                S("vapour", M.Drift, F, 0.4f, 0.15f, 0.9f, 0.7f)),
            Slower(T("dark_poison", "the mites spiral in and the black mouth spreads; the lobed front laps and bursts; fumes hook inward",
                S("swirl", M.Spin, A, 0f, 0.45f, 0.9f, -260f),
                S("mark", M.Spread, A, 0.05f, 0.4f, 1.3f, 0.35f),
                S("ring", M.Pulse, I, 0.35f, 0.08f, 0.8f, 2f),
                S("vapour", M.Drift, F, 0.45f, 0.15f, 0.85f, 0.5f)), SlowPace),
            // The owner's review (2026-11-19): no big burst (the builder leaves its rings out); small jolts over the body
            // here, and again at every bite of the poison (AfflictionArt.Jolt).
            T("poison_lightning", "jagged nerve lines strobe up out of the body; static jolts over it; the green fern spreads",
                S("nerveruyi", M.Rise, A, 0f, 0.2f, 0.8f, 0f, 3),
                S("static", M.Flicker, I, 0.2f, 0.3f, 0.6f, 120f, 4, true),
                S("ring", M.Pulse, I, 0.28f, 0.06f, 0.8f, 4f),
                S("mark", M.Spread, F, 0.3f, 0.35f, 1f, 0.4f)),
            // The owner's review (2026-11-19): the ground needs a splash outward to feel like water: Water's approved
            // after-splash and crown, borrowed by the builder, rise round the pit as the slabs land.
            T("water_earth", "the slabs drop and tip in round the pit; water splashes up and out round it; the funnel and collar open; water seeps up in rings",
                // Held a metre up so the drop is seen from the game's camera (0.45 m read as standing still).
                S("growth", M.Sink, A, 0f, 0.4f, 1.1f, 1f),
                S("wave", M.Rise, I, 0.3f, 0.22f, 0.9f),
                S("mark", M.Spread, I, 0.25f, 0.5f, 1.15f, 0.3f),
                S("ring", M.Pulse, F, 0.45f, 0.1f, 0.8f, 2f)),
            T("wind_earth", "wind sweeps in from the side; the rut ring heaves up and dust bursts; the comb spreads",
                S("wind", M.Sweep, A, 0f, 0.3f, 0.75f, 2.5f),
                S("growth", M.Rise, I, 0.25f, 0.25f, 0.9f),
                S("debris", M.Burst, I, 0.3f, 0.22f, 0.65f),
                S("mark", M.Spread, F, 0.25f, 0.4f, 1f, 0.35f)),
            // The owner's review (2026-11-19): the anvil springs up out of the ground (from 1.2 m under it) in the middle
            // of the look, under the one struck (the builder centres it).
            T("earth_metal", "the anvil springs up out of the ground under it and the ground heaves; the plates clang with a white-hot notch; shards and dust fly; the clang rings spread",
                S("anvil", M.Rise, A, 0f, 0.3f, 1.1f, 1.2f),
                S("growth", M.Rise, A, 0f, 0.28f, 1f),
                S("ring", M.Pulse, I, 0.26f, 0.06f, 0.85f, 3f, 0, true),
                S("debris", M.Burst, I, 0.28f, 0.22f, 0.7f, 0f, 0, true),
                S("mark", M.Spread, F, 0.3f, 0.35f, 1f, 0.35f)),
            T("life_metal", "iron roots creep out and vines rise; the plates clang; steel thorns burst out of them; leaves and sparks fly",
                S("thorn", M.Burst, I, 0.25f, 0.2f, 0.85f, 0f, 0, true),
                S("mark", M.Spread, A, 0f, 0.4f, 1.2f, 0.3f),
                S("growth", M.Rise, A, 0.05f, 0.28f, 1f),
                S("ring", M.Pulse, I, 0.22f, 0.06f, 0.8f, 2f, 0, true),
                S("debris", M.Burst, F, 0.35f, 0.25f, 0.7f)),
            // The owner's review (2026-11-19): the coils turning round the body and the pale streaks on the floor read as
            // wind round the black roots; the streaks are left out and the coils grow up the body without turning.
            T("life_dark", "black fig roots grip the floor; briars and vines climb; pale coils grow up round the body; leaves fall away",
                S("mark", M.Spread, A, 0f, 0.45f, 1.3f, 0.3f),
                S("growth", M.Rise, A, 0.08f, 0.3f, 1f),
                S("coils", M.Rise, I, 0.28f, 0.3f, 0.9f),
                S("debris", M.Burst, F, 0.4f, 0.25f, 0.7f)),
            T("fire_water", "flame stubs flicker on the body; the water lands and the soot ring spreads; the splash rises; steam billows up and thins",
                S("flame", M.Lick, A, 0f, 0.15f, 0.45f),
                S("mark", M.Spread, I, 0.15f, 0.3f, 1.2f, 0.4f),
                S("wave", M.Rise, I, 0.15f, 0.2f, 0.65f),
                S("debris", M.Burst, I, 0.18f, 0.2f, 0.55f),
                S("vapour", M.Drift, F, 0.25f, 0.2f, 1.15f, 1.3f)),
            T("metal_light", "the slatted shaft strikes down; the plates clang and glint and sparks fork; the split beam runs to the next body; the jali print and shadow spread",
                S("strike", M.Fall, A, 0f, 0.16f, 0.6f, 5f, 0, true),
                S("ring", M.Pulse, I, 0.16f, 0.06f, 0.85f, 3f, 0, true),
                S("debris", M.Burst, I, 0.18f, 0.2f, 0.6f, 0f, 0, true),
                S("ray", M.Extend, I, 0.24f, 0.28f, 0.85f, 0f, 0, true),
                S("mark", M.Spread, F, 0.4f, 0.35f, 0.95f, 0.3f)),
            T("fire_life", "the sprout and vines rise; the lilies open; flames lick up through them in turn; coal pellets burst; embers and flakes rise as the ash rings spread",
                S("flower", M.Rise, I, 0.2f, 0.22f, 0.95f, 0f, 0, true),
                S("growth", M.Rise, A, 0f, 0.28f, 1f),
                S("flame", M.Lick, I, 0.25f, 0.16f, 0.85f, 0f, 0, false, 0.08f),
                S("debris", M.Burst, I, 0.3f, 0.2f, 0.6f),
                S("vapour", M.Drift, F, 0.45f, 0.15f, 0.85f, 0.9f),
                S("mark", M.Spread, F, 0.25f, 0.35f, 1.1f, 0.35f)),
            T("lightning_earth", "bolts fall and strobe and static crawls on every body; the burnt fern spreads; cubes jump off it and the heave ring rises; dust bursts",
                S("strike", M.Fall, A, 0f, 0.15f, 0.55f, 5f, 2, true),
                S("static", M.Flicker, A, 0.05f, 0.35f, 0.75f, 0f, 4, true),
                S("mark", M.Spread, I, 0.15f, 0.25f, 1.1f, 0.3f),
                S("growth", M.Rise, I, 0.22f, 0.22f, 0.9f),
                S("debris", M.Burst, F, 0.25f, 0.25f, 0.7f)),
            T("dark_ice", "frost crusts the body and chips spring off; the breath draws out toward you puff by puff to the warmth ring; the rosettes spread along the chain",
                S("growth", M.Rise, A, 0f, 0.25f, 1.1f),
                S("debris", M.Burst, A, 0.05f, 0.22f, 0.6f, 0f, 0, true),
                S("ray", M.Extend, I, 0.18f, 0.45f, 1f),
                S("mark", M.Spread, F, 0.3f, 0.45f, 1f, 0.3f)),
            T("light_lightning", "a thread of light: the gold rings spread; the straight bolt draws down it with two strobe redraws; static crawls; the straight scorch is left",
                S("mark", M.Spread, A, 0f, 0.3f, 1.1f, 0.3f),
                S("strike", M.Extend, I, 0.12f, 0.14f, 0.6f, 0f, 2, true),
                S("static", M.Flicker, F, 0.22f, 0.3f, 0.55f, 0f, 3)),
        };

        /// <summary>A designed look's timeline by its recipe key; false for none.</summary>
        public static bool TryGet(string key, out Timeline timeline)
        {
            foreach (var one in Designed)
                if (one.Key == key)
                {
                    timeline = one;
                    return true;
                }
            timeline = default;
            return false;
        }

        /// <summary>
        /// The step a piece of a look follows: the first naming a word of its kit object's name, else the first naming
        /// its role. -1 (and the role's default cue) when the timeline names neither, or the look has none.
        /// </summary>
        public static int StepFor(string key, string kitObject, byte role, out Step step)
        {
            if (TryGet(key, out var timeline))
            {
                string name = Plain(kitObject);
                for (int i = 0; i < timeline.Steps.Length; i++)
                {
                    string match = timeline.Steps[i].Match;
                    if (RoleNamed(match) == 255 && name.Contains(match))
                    {
                        step = timeline.Steps[i];
                        return i;
                    }
                }
                for (int i = 0; i < timeline.Steps.Length; i++)
                    if (RoleNamed(timeline.Steps[i].Match) == role)
                    {
                        step = timeline.Steps[i];
                        return i;
                    }
            }
            step = new Step { Match = RoleNames[role < RoleNames.Length ? role : 0], Cue = Default(role) };
            return -1;
        }

        /// <summary>True when the steps hold a beat of the cause arriving and one of the impact.</summary>
        public static bool HasArrivalAndImpact(Step[] steps)
        {
            bool arrives = false, lands = false;
            foreach (var step in steps)
            {
                arrives |= step.Cue.Beat == M.Arrive;
                lands |= step.Cue.Beat == M.Impact;
            }
            return arrives && lands;
        }

        // ---- The 31 composed looks: the first parent's piece arrives by its element's nature, the second answers at
        // the impact, and the ground mark spreads after.

        /// <summary>
        /// How an element's own piece moves in a composed look: Lightning falls and strobes, Wind sweeps in from the
        /// player, Water, Earth and Life rise, Fire licks, Poison and Dark spread, Light sweeps in as a ray, Ice crusts
        /// (spreads), Metal clangs (pulses). Given at the arrival beat, from the start.
        /// </summary>
        public static Cue Nature(int element)
        {
            switch (element)
            {
                case 0: return Cue.Of(M.Lick, M.Arrive, 0f, 0.18f, 0.85f);
                case 1: return Cue.Of(M.Fall, M.Arrive, 0f, 0.16f, 0.6f, 4.5f, 2, true);
                case 2: return Cue.Of(M.Sweep, M.Arrive, 0f, 0.3f, 0.75f, 2.5f);
                case 3: return Cue.Of(M.Spread, M.Arrive, 0f, 0.3f, 0.85f, 0.3f, 0, true);
                case 4: return Cue.Of(M.Rise, M.Arrive, 0f, 0.22f, 0.8f);
                case 5: return Cue.Of(M.Rise, M.Arrive, 0f, 0.28f, 0.85f);
                case 6: return Cue.Of(M.Rise, M.Arrive, 0f, 0.22f, 0.85f);
                case 7: return Cue.Of(M.Pulse, M.Arrive, 0f, 0.08f, 0.75f, 3f, 0, true);
                case 8: return Cue.Of(M.Sweep, M.Arrive, 0f, 0.2f, 0.7f, 3f, 0, true);
                case 9: return Cue.Of(M.Spread, M.Arrive, 0f, 0.3f, 0.85f, 0.2f);
                case 10: return Cue.Of(M.Spread, M.Arrive, 0f, 0.3f, 0.85f, 0.2f);
                default: return Cue.Of(M.Pop, M.Arrive, 0f, 0.1f, 0.8f);
            }
        }

        /// <summary>
        /// A composed look's three cues: the first parent's piece arriving by its nature, the second's answering it at
        /// the impact (when the first lands, or a fifth of a second in for one that grows where it is), and the ground
        /// mark spreading after.
        /// </summary>
        public static void Composed(int pair, out Cue first, out Cue second, out Cue mark)
        {
            int a = ReactionDesigns.RingIndex(AfflictionPairs.First(pair)), b = ReactionDesigns.RingIndex(AfflictionPairs.Second(pair));
            first = Nature(a);
            float impact = M.Lands(first.Verb) ? Mathf.Max(0.16f, first.Move) : 0.22f;
            second = Nature(b);
            second.Beat = M.Impact;
            second.Delay = impact;
            second.Move *= 0.8f;
            second.Seconds *= 0.9f;
            mark = Cue.Of(M.Spread, M.Aftermath, impact + 0.08f, 0.35f, 1.15f, 0.7f);
            // Seized's lightning crackles without flashing, so it does not hurt the eyes (the owner, 2026-11-20): no
            // strobe, no white frame, a new shape three times a second, held on through.
            if (pair == ReactionDesigns.Seized)
            {
                var calm = Cue.OfCrackle(M.Arrive, 0f, 0.14f, 1.35f, 72f);
                if (a == ReactionDesigns.RingIndex("lightning")) first = calm;
                else
                {
                    calm.Beat = M.Impact;
                    calm.Delay = second.Delay;
                    second = calm;
                }
            }
        }
    }
}
