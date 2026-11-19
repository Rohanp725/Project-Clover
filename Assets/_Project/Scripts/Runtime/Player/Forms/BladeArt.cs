using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The charged blade in the approved look (the look-dev's "charged" panels), for <see cref="BladeLook"/>: what stands
    /// on the edge while the charge lasts and what each swing leaves. Fire keeps its tongues on the edge and peels a
    /// sheared flame sheet with flakes off each swing; Lightning's crackle jumps from the tip; Wind's streaks peel back
    /// off it; Ice wears rime feathers along its back; Water throws a curtain of drops on the follow-through; Life has a
    /// vine wound round it; Earth's swing drags a furrow; Metal's edge carries a chrome band and its trail is cut off
    /// square, sparks only where it meets the floor; Light's edge is beaten gold, its last cuts hang redrawn by little
    /// suns; Dark's edge is cut out of the world, a floor shadow lags the swing and ink rings drop off it; Poison's venom
    /// necks into strings and its trail is a centipede of jointed plates.
    /// </summary>
    public static class BladeArt
    {
        /// <summary>The piece that stands along the edge in the element art; null where the built shape stays (flame, bolt, air, water, grit).</summary>
        public static string EdgePiece(int element)
        {
            switch (element)
            {
                case ElementSubstance.Ice: return "ice_rime_charged";
                case ElementSubstance.Life: return "life_bladevine_charged";
                case ElementSubstance.Metal: return "metal_bladeband_charged";
                case ElementSubstance.Light: return "light_gilt_charged";
                case ElementSubstance.Dark: return "dark_edge_charged";
                case ElementSubstance.Poison: return "poison_venom_charged";
                default: return null;
            }
        }

        /// <summary>How long an edge piece was drawn (its length along the kit's X), so it can be fitted to the edge.</summary>
        public static float EdgeLength(int element)
        {
            int piece = ElementArt.Piece(EdgePiece(element) ?? "");
            return piece >= 0 ? Mathf.Max(0.1f, ElementArt.SizeOf(piece).x) : 1f;
        }

        /// <summary>An edge piece laid along the blade: the kit's X along the edge, its Y standing off the flat of it.</summary>
        public static Quaternion Along(Vector3 direction, int index) =>
            Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, -90f, index * 25f);

        // The cut of a charged or infused blow in each element's own band (Fire's orange, Poison's jade, Dark's violet rim),
        // bright enough to read against the floor but never pushed into white by the bloom.
        private static readonly Color[] Arcs =
        {
            new Color(1.25f, 0.34f, 0.05f, 0.95f), new Color(0.55f, 0.75f, 1.35f, 0.95f), new Color(0.95f, 0.97f, 0.9f, 0.9f),
            new Color(0.55f, 0.95f, 1.2f, 0.95f), new Color(0.1f, 0.75f, 0.8f, 0.95f), new Color(0.35f, 0.9f, 0.2f, 0.95f),
            new Color(0.72f, 0.48f, 0.2f, 0.95f), new Color(0.88f, 0.9f, 0.96f, 0.95f), new Color(1.2f, 0.92f, 0.45f, 0.95f),
            new Color(0.5f, 0.22f, 0.85f, 0.95f), new Color(0.35f, 0.75f, 0.55f, 0.95f),
        };

        /// <summary>The colour a charged or infused cut of the element is drawn in.</summary>
        public static Color ArcColour(int element) => element >= 0 && element < Arcs.Length ? Arcs[element] : Color.white;

        /// <summary>What one swing of a charged blade leaves, from where the player stands, facing the way given.</summary>
        public static void Swing(ElementInfo element, Vector3 origin, Vector3 forward)
        {
            if (!ElementArt.Ready || element == null) return;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;
            forward.Normalize();
            float yaw = ElementArt.Turn(forward);
            Vector3 ahead = origin + forward * 1f;
            switch (element.Index)
            {
                case ElementSubstance.Fire:
                    ElementArt.Stand(ElementArt.Piece("fire_flakes_charged"), ahead, yaw, 1.1f, 0.4f);
                    ElementArt.Sheet(ElementArt.SheetOf("fire_flame_loop"), ahead + Vector3.up * 0.9f, 1.6f, 0.4f);
                    break;
                case ElementSubstance.Lightning:
                    ElementArt.Stand(ElementArt.Piece("lightning_jump_charged"), origin + forward * 0.6f, yaw, 1f, 0.07f);
                    break;
                case ElementSubstance.Wind:
                    ElementArt.Stand(ElementArt.Piece("wind_peel_charged"), ahead, yaw, 1f, 0.3f);
                    break;
                case ElementSubstance.Water:
                    ElementArt.Stand(ElementArt.Piece("water_curtain_charged"), ahead, yaw, 1f, 0.35f);
                    break;
                case ElementSubstance.Earth:
                    ElementArt.Stand(ElementArt.Piece("earth_furrow_charged"), ahead, yaw, 1f, 1.2f);
                    break;
                case ElementSubstance.Metal:
                    ElementArt.Stand(ElementArt.Piece("metal_trail_charged"), ahead, yaw, 1f, 0.18f);
                    ElementArt.Stand(ElementArt.Piece("metal_sparks_charged"), origin + forward * 1.6f, yaw, 1f, 0.14f);
                    break;
                case ElementSubstance.Light:
                    // The last cuts hang in the air, each being redrawn by a small travelling sun.
                    ElementArt.Stand(ElementArt.Piece("light_cut_charged"), ahead, yaw, 1f, 0.4f);
                    ElementArt.Stand(ElementArt.Piece("light_cutburn_charged"), ahead + forward * 0.5f + Vector3.up * 0.9f, yaw, 1.2f, 0.4f);
                    ElementArt.Stand(ElementArt.Piece("light_bead_charged"), origin + Vector3.up * 1.1f + forward * 0.4f, yaw, 0.6f, 0.25f);
                    break;
                case ElementSubstance.Dark:
                    ElementArt.Floor(ElementArt.Piece("dark_floortrail_charged"), ahead, yaw, 1f, 1f, 0.6f);
                    ElementArt.Drop(ElementArt.Piece("dark_inkring_charged"), ahead + Vector3.up * 1.1f, ahead + Vector3.up * 0.1f, Quaternion.identity, 1f, 0.7f, 0.1f, 0.2f);
                    break;
                case ElementSubstance.Poison:
                    ElementArt.Stand(ElementArt.Piece("poison_centipede_charged"), ahead, yaw, 1f, 0.35f);
                    ElementArt.Floor(ElementArt.Piece("poison_pits_charged"), ahead + forward * 0.5f, yaw, 1f, 1f, 1.5f);
                    break;
            }
        }
    }
}
