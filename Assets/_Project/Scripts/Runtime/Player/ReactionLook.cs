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
        public static void Play(int pair, Vector3 at, Transform body, Vector3 away) => Play(pair, at, body, away, null);

        /// <summary>The same, with the player the reaction's own part on the player (if it has one) is drawn on.</summary>
        public static void Play(int pair, Vector3 at, Transform body, Vector3 away, Transform player)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return;
            if (Forms.ElementArt.Ready)
            {
                // The bespoke look made for the pair (the Blender worker's combos 4, the owner's review of 2026-11-20: "breaking
                // away from the copy paste effect concept"), where it has been built; else the earlier designed or composed look.
                if (body != null && Forms.ElementLooks.PlayReaction(SlugOf(pair), body, away, player,
                    (1 << (int)AfflictionPairs.First(pair)) | (1 << (int)AfflictionPairs.Second(pair))))
                {
                    Bespoke++;
                    return;
                }
                if (!Forms.ReactionArt.Play(pair, at, body, away)) Forms.ReactionArt.Compose(pair, at, body, away);
                // Rime's crust builds up slowly over the body, layer on layer, and its feathers grow into the wind (the
                // owner, 2026-11-20: "the rime instantly all appears instead of slowly building").
                if (pair == Rime && body != null) Forms.ElementLooks.Play(Forms.ElementLooks.Find("ice_rime"), body);
                return;
            }
            Vfx.Play(VfxKind.TintPuff, at + Vector3.up * 1.3f, Vector3.up, 1.1f, Colour(pair));
        }

        /// <summary>
        /// True when a reaction mark's pair has a bespoke look built (combos 4): its own bursts and lanes are then drawn by
        /// that look, not by a plain burst of its element (Fire Medicine's fireball, Smoulder's orange flare, 2026-11-20).
        /// </summary>
        public static bool HasBespoke(Upgrade owner)
        {
            if (owner == null || owner.Kind != UpgradeKind.Reaction || !Forms.ElementArt.Ready) return false;
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                if (ReferenceEquals(owner.Id, AfflictionPairs.Id(pair)) || owner.Id == AfflictionPairs.Id(pair)) return Forms.ElementLooks.Find(SlugOf(pair)) >= 0;
            return false;
        }

        /// <summary>Reactions drawn by their bespoke look this play, for probes.</summary>
        public static int Bespoke { get; private set; }

        // Each pair's slug in the recipes ("Drowned Weight" is drowned_weight), made once.
        private static string[] slugs;

        /// <summary>The pair's name as the recipes write it: lower case, words joined by underscores, nothing else.</summary>
        public static string SlugOf(int pair)
        {
            if (slugs == null)
            {
                slugs = new string[AfflictionPairs.Count];
                for (int i = 0; i < slugs.Length; i++)
                {
                    var text = new System.Text.StringBuilder();
                    foreach (char c in AfflictionPairs.Name(i).ToLowerInvariant())
                    {
                        if (char.IsLetterOrDigit(c)) text.Append(c);
                        else if (c == ' ' || c == '-') text.Append('_');
                    }
                    slugs[i] = Renamed(text.ToString());
                }
            }
            return pair >= 0 && pair < slugs.Length ? slugs[pair] : "";
        }

        // Reactions the look-dev renamed (staging/blender/2026-11-19_combos3/notes.md): their looks are under the new names.
        private static string Renamed(string slug)
        {
            switch (slug)
            {
                case "windthrow": return "seed_storm";
                case "split_rock": return "root_snare";
                case "burning_sap": return "sun_sap";
                default: return slug;
            }
        }

        // Domain reload is off: the count starts again each play (the slugs are fixed).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Bespoke = 0;

        /// <summary>Rime: Unsteady and Chill.</summary>
        public static readonly int Rime = AfflictionPairs.Index(AfflictionKind.Unsteady, AfflictionKind.Chill);

        /// <summary>The pair's two colours, half and half: the colour its name is written in.</summary>
        public static Color Colour(int pair) =>
            Color.Lerp(Afflictions.Spec(AfflictionPairs.First(pair)).Colour, Afflictions.Spec(AfflictionPairs.Second(pair)).Colour, 0.5f);
    }
}
